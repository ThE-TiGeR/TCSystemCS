﻿// *******************************************************************************
// 
//  *******   ***   ***               *
//     *     *     *                  *
//     *    *      *                *****
//     *    *       ***  *   *   **   *    **    ***
//     *    *          *  * *   *     *   ****  * * *
//     *     *         *   *      *   * * *     * * *
//     *      ***   ***    *     **   **   **   *   *
//                         *
// *******************************************************************************
//  see https://github.com/ThE-TiGeR/TCSystemCS for details.
//  Copyright (C) 2003 - 2021 Thomas Goessler. All Rights Reserved.
// *******************************************************************************
// 
//  TCSystem is the legal property of its developers.
//  Please refer to the COPYRIGHT file distributed with this source distribution.
// 
// *******************************************************************************

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using TCSystem.MetaData;

#endregion

namespace TCSystem.MetaDataDB
{
    internal sealed class DB2Files : DB2Constants
    {
#region Public

        public long GetNumFiles()
        {
            using (var command = new SqliteCommand
            {
                Connection = Instance.Connection,
                CommandText = $"SELECT COUNT({IdFileId}) FROM {TableFiles};"
            })
            {
                var result = command.ExecuteScalar();
                return (long?) result ?? 0;
            }
        }

        public IList<string> GetAllFilesLike(string filter)
        {
            var filterFilesCommand = $"WHERE {IdFileName} LIKE @Filter {LikeEscape} ";

            var filterCommand = filter != null ? filterFilesCommand : "";
            if (filter != null)
            {
                filter = EscapeSpecialLikeCharacters(filter);
                filter += "%";
            }

            using (var command = new SqliteCommand
            {
                Connection = Instance.Connection,
                CommandText = $"SELECT {IdFileName} FROM {TableFiles}" +
                              $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFiles}.{IdFileId} " +
                              filterCommand +
                              $"ORDER by {TableFileData}.{IdDateTaken} DESC;"
            })
            {
                command.Parameters.AddWithValue("@Filter", filter);

                using (var reader = command.ExecuteReader())
                {
                    var files = new List<string>();
                    while (reader.Read())
                    {
                        files.Add(reader.GetString(0));
                    }

                    return files;
                }
            }
        }

        public long GetFileId(string fileName, SqliteTransaction transaction)
        {
            using (var command = new SqliteCommand
            {
                Transaction = transaction,
                Connection = Instance.Connection,
                CommandText = $"SELECT {IdFileId} FROM {TableFiles} WHERE {IdFileName}=@{IdFileName};"
            })
            {
                command.Parameters.AddWithValue($"@{IdFileName}", fileName);
                var result = command.ExecuteScalar();
                return (long?) result ?? Constants.InvalidId;
            }
        }

        public long AddFile(Image data, DateTimeOffset dateModified, SqliteTransaction transaction)
        {
            var fileId = GetFileId(data.FileName, transaction);
            if (fileId == Constants.InvalidId)
            {
                using (var command = new SqliteCommand
                {
                    Transaction = transaction,
                    Connection = Instance.Connection,
                    CommandText = $"INSERT INTO {TableFiles} ({IdFileName}, {IdDateModified}, {IdProcessingInfo}) " +
                                  $"VALUES (@{IdFileName}, @{IdDateModified}, @{IdProcessingInfo});"
                })
                {
                    command.Parameters.AddWithValue($"@{IdFileName}", data.FileName);
                    command.Parameters.AddWithValue($"@{IdDateModified}", dateModified.ToString("s"));
                    command.Parameters.AddWithValue($"@{IdProcessingInfo}", (long) data.ProcessingInfos);
                    command.ExecuteNonQuery();
                }

                fileId = GetFileId(data.FileName, transaction);
            }

            return fileId;
        }

        public void RemoveFile(long fileId, SqliteTransaction transaction)
        {
            using (var command = new SqliteCommand
            {
                Transaction = transaction,
                Connection = Instance.Connection,
                CommandText = $"DELETE From {TableFiles} WHERE {IdFileId}=@{IdFileId};"
            })
            {
                command.Parameters.AddWithValue($"@{IdFileId}", fileId);
                command.ExecuteNonQuery();
            }
        }

        public DateTimeOffset GetDateModified(string fileName)
        {
            using (var command = new SqliteCommand
            {
                Connection = Instance.Connection,
                CommandText = $"SELECT {IdDateModified} FROM {TableFiles} " +
                              $"WHERE {IdFileName}=@{IdFileName};"
            })
            {
                command.Parameters.AddWithValue($"@{IdFileName}", fileName);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return reader.GetDateTimeOffset(0);
                    }

                    return DateTimeOffset.MinValue;
                }
            }
        }

        public long GetNumAllFilesLike(string folder)
        {
            var filterFilesCommand = $"WHERE {IdFileName} LIKE @Filter {LikeEscape} ";

            var filterCommand = folder != null ? filterFilesCommand : "";
            if (folder != null)
            {
                folder += "%";
            }

            using (var command = new SqliteCommand
            {
                Connection = Instance.Connection,
                CommandText = $"SELECT COUNT({IdFileName}) FROM {TableFiles}" +
                              $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFiles}.{IdFileId} " +
                              filterCommand +
                              $"ORDER by {TableFileData}.{IdDateTaken} DESC;"
            })
            {
                command.Parameters.AddWithValue("@Filter", folder);
                var result = command.ExecuteScalar();
                return (long?) result ?? 0;
            }
        }

        public IList<(string FileName, ProcessingInfos ProcessingInfo)> GetAllProcessingInformation()
        {
            using (var command = new SqliteCommand
            {
                Connection = Instance.Connection,
                CommandText = $"SELECT {IdFileName}, {IdProcessingInfo} FROM {TableFiles}" +
                              $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFiles}.{IdFileId} " +
                              $"ORDER by {TableFileData}.{IdDateTaken} DESC;"
            })
            {
                using (var reader = command.ExecuteReader())
                {
                    var values = new List<(string FileName, ProcessingInfos ProcessingInfo)>();
                    while (reader.HasRows && reader.Read())
                    {
                        values.Add((reader.GetString(0), (ProcessingInfos) reader.GetInt64(1)));
                    }

                    return values;
                }
            }
        }

        public IDictionary<string, DateTimeOffset> GetAllFileAndModifiedDates()
        {
            using (var command = new SqliteCommand
            {
                Connection = Instance.Connection,
                CommandText = $"SELECT {IdFileName}, {IdDateModified} FROM {TableFiles};"
            })
            {
                using (var reader = command.ExecuteReader())
                {
                    var values = new Dictionary<string, DateTimeOffset>();
                    while (reader.HasRows && reader.Read())
                    {
                        values.Add(reader.GetString(0), reader.GetDateTimeOffset(1));
                    }

                    return values;
                }
            }
        }

        public IList<string> SearchForFiles(string searchFilterString)
        {
            var searchFilters = PrepareFilters(searchFilterString);
            if (searchFilters.Count == 0)
            {
                searchFilters = _filters.Select(f => (f, search_filter_string: searchFilterString, false)).ToArray();
            }

            var commandText = string.Empty;
            foreach (var (key, value, isAdd) in searchFilters)
            {
                var nextCommand = key switch
                {
                    FilterFile => $"SELECT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFiles}\n " +
                                  $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFiles}.{IdFileId}\n " +
                                  $"WHERE {IdFileName} LIKE '%{value}%' {LikeEscape}\n ",

                    FilterDate => $"SELECT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFileData}\n " +
                                  $"    INNER JOIN {TableFiles} ON {TableFiles}.{IdFileId}={TableFileData}.{IdFileId}\n " +
                                  $"WHERE {IdDateTaken} LIKE '%{value}%' {LikeEscape}\n ",

                    FilterLocation => $"SELECT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFileLocations}\n " +
                                      $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFileLocations}.{IdFileId}\n " +
                                      $"    INNER JOIN {TableFiles} ON {TableFiles}.{IdFileId}={TableFileLocations}.{IdFileId}\n " +
                                      $"    INNER JOIN {TableLocations} ON {TableLocations}.{IdLocationId}={TableFileLocations}.{IdLocationId}\n " +
                                      $"WHERE {TableLocations}.{IdCountry} LIKE '%{value}%' {LikeEscape} OR\n " +
                                      $"      {TableLocations}.{IdProvince} LIKE '%{value}%' {LikeEscape} OR\n " +
                                      $"      {TableLocations}.{IdCity} LIKE '%{value}%' {LikeEscape} OR\n " +
                                      $"      {TableLocations}.{IdStreet} LIKE '%{value}%' {LikeEscape}\n ",

                    FilterName => $"SELECT DISTINCT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFileFaces}\n " +
                                  $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFileFaces}.{IdFileId}\n " +
                                  $"    INNER JOIN {TableFiles} ON {TableFiles}.{IdFileId}={TableFileFaces}.{IdFileId}\n " +
                                  $"    INNER JOIN {TablePersons} ON {TablePersons}.{IdPersonId}={TableFileFaces}.{IdPersonId}\n " +
                                  $"WHERE {IdName} LIKE '%{value}%' {LikeEscape}\n ",

                    FilterTag => $"SELECT DISTINCT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFileTags}\n " +
                                 $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFileTags}.{IdFileId}\n " +
                                 $"    INNER JOIN {TableFiles} ON {TableFiles}.{IdFileId}={TableFileTags}.{IdFileId}\n " +
                                 $"    INNER JOIN {TableTags} ON {TableTags}.{IdTagId}={TableFileTags}.{IdTagId}\n " +
                                 $"WHERE {IdTag} LIKE '%{value}%' {LikeEscape}\n ",

                    FilterFaceId => $"SELECT DISTINCT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFileFaces}\n " +
                                    $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFileFaces}.{IdFileId}\n " +
                                    $"    INNER JOIN {TableFiles} ON {TableFiles}.{IdFileId}={TableFileFaces}.{IdFileId}\n " +
                                    $"    INNER JOIN {TablePersons} ON {TablePersons}.{IdPersonId}={TableFileFaces}.{IdPersonId}\n " +
                                    $"WHERE {IdFaceId}='{value}'\n ",

                    FilterNumPersons when int.TryParse(value, out var _) =>
                        $"SELECT DISTINCT {IdFileName}, {TableFileData}.{IdDateTaken} FROM {TableFileFaces}\n " +
                        $"    INNER JOIN {TableFileData} ON {TableFileData}.{IdFileId}={TableFileFaces}.{IdFileId}\n " +
                        $"    INNER JOIN {TableFiles} ON {TableFiles}.{IdFileId}={TableFileFaces}.{IdFileId}\n " +
                        $"GROUP BY {IdFileName}\n " +
                        $"HAVING COUNT(*) == {value}\n ",

                    _ => null
                };

                if (nextCommand != null)
                {
                    if (!string.IsNullOrEmpty(commandText))
                    {
                        commandText += isAdd ? "INTERSECT\n " : "UNION\n ";
                    }

                    commandText += nextCommand;
                }
            }


            var files = new List<string>();
            if (!string.IsNullOrEmpty(commandText))
            {
                commandText += $"ORDER by {TableFileData}.{IdDateTaken} DESC;";
                using var command = new SqliteCommand {Connection = Instance.Connection, CommandText = commandText};
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    files.Add(reader.GetString(0));
                }
            }

            return files;
        }

        public void RemoveAllFilesOfFolder(string folder)
        {
            using (var transaction = Instance.BeginTransaction())
            {
                using (var command = new SqliteCommand
                {
                    Transaction = transaction,
                    Connection = Instance.Connection,
                    CommandText = $"DELETE From {TableFiles} WHERE {IdFileName} LIKE @Folder {LikeEscape};"
                })
                {
                    folder = EscapeSpecialLikeCharacters(folder);
                    command.Parameters.AddWithValue("@Folder", folder + "%");
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }

        public DB2Instance Instance;

#endregion

#region Private

        private static IReadOnlyList<(string Key, string Value, bool IsAdd)> PrepareFilters(string searchFilterString)
        {
            var searchFiltersList = new List<(string Key, string Value, bool IsAdd)>();

            string key;
            string value;

            var isAdd = true;
            var startKey = 0;
            var endKey = 0;
            var startValue = 0;
            int endValue;
            for (var index = 0; index < searchFilterString.Length; index++)
            {
                var v = searchFilterString[index];
                if (v == '=')
                {
                    endKey = index - 1;
                    startValue = index + 1;
                }
                else if (v == '&' || v == '|')
                {
                    endValue = index - 1;
                    (key, value) = ExtractKeyValue(searchFilterString, startKey, endKey, startValue, endValue);
                    if (key != null)
                    {
                        searchFiltersList.Add((key, EscapeSpecialLikeCharacters(value), isAdd));
                        isAdd = v == '&';
                    }

                    startKey = index + 1;
                }
            }

            endValue = searchFilterString.Length - 1;
            (key, value) = ExtractKeyValue(searchFilterString, startKey, endKey, startValue, endValue);
            if (key != null)
            {
                searchFiltersList.Add((key, EscapeSpecialLikeCharacters(value), isAdd));
            }

            return searchFiltersList;
        }

        private static (string Key, string Value) ExtractKeyValue(string searchFilterString, int startKey, int endKey, int startValue, int endValue)
        {
            if (startKey < endKey && endKey < startValue)
            {
                var key = searchFilterString.Substring(startKey, endKey + 1 - startKey);
                if (startValue <= endValue)
                {
                    var value = searchFilterString.Substring(startValue, endValue + 1 - startValue);
                    return (key, value);
                }
            }

            return (null, null);
        }

        private static string EscapeSpecialLikeCharacters(string text)
        {
            return text.Replace("_", $"{EscapeCharacter}_")
                .Replace("'", "");
        }

#endregion
    }
}
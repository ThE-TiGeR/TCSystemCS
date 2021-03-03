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
using System.Threading.Tasks;

#endregion

namespace TCSystem.Thread
{
    public interface IWorkerThread
    {
#region Public

        void ExecuteCommand(Action action);
        void ExecuteCommand(Action action, string message);
        void ClearOpenCommands();

        Task ExecuteCommandAsync(Action action);
        Task ExecuteCommandAsync(Action action, string message);
        Task ClearOpenCommandsAsync();

        void StopThread();

        int NumOpenActions { get; }
        bool IsBusy { get; }

#endregion
    }
}
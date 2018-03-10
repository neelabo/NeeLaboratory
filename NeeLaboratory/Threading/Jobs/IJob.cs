﻿using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Jobインターフェイス
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Jobの実行処理
        /// </summary>
        Task ExecuteAsync();
    }
}

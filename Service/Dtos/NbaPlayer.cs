using System;
using System.Collections.Generic;
using System.Text;
using Service.Implements;

namespace Service.Dtos
{
    /// <summary>
    /// 球員生涯資料
    /// </summary>
    public class NbaPlayer
    {
        public string NameType { get; set; }

        public Career Career = new Career();
    }
}
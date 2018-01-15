﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WechatCommon.Data
{
    public class WechatRequest
    {
        public string ToUserName { get; set; } = "";
        public string FromUserName { get; set; } = "";
        public string CreateTime { get; set; } = "";

        public string MsgType { get; set; } = "";
        public string Content { get; set; } = "";
        public string MsgId { get; set; } = "";
    }
}

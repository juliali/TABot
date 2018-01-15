using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Common.Utils;

namespace Bot.Common.Data
{
    public class WBContext : AbstractContext
    {       
        public string Location;       

        public WBContext(string userId)
        {            
            this.userId = userId;
        }

        public bool IsValid()
        {
            DateTime nowTime = DateTime.Now;

            if (validTime == null)
            {
                return false;
            }
            else if (nowTime.Subtract(validTime).TotalMinutes > 30)
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(Location))
            {
                return false;
            }
            else
            {
                return true;
            }
        }                
    }
}

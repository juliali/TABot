using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WechatBotFramework.Controllers;
using WechatBotFramework.Data;

namespace WechatBotFramework.Component
{
    public class MessageHandler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TAController controller = new TAController();

        public ReplyInfo HandleMessageAll(MessageInfo message)
        {
            ReplyInfo info = new ReplyInfo();
                                    
            switch(message.msg_type_id)
            {
                case 4: // msg from contact
                    //info.toUserId = message.user.senderId;
                    if (message.content.type == 0)
                    {                        
                        string msg_content = message.content.data;
                        string user_id = message.user.senderId;
                        string user_name = message.user.senderName;

                        log.Info("[Contact Message]: From (" + user_name + "), Context: " + msg_content);

                        if((user_name == "YJL SOEVPM") && (msg_content.StartsWith("##")))
                        {
                            string[] tmps = msg_content.Split('@');
                            string notifier = user_name;
                            if (tmps.Length > 1)
                            {
                                notifier = tmps[0].Replace("##", "");
                            }

                            string content = tmps[tmps.Length - 1];

                            NotifyResponse resp = controller.NotificationStore(notifier, content);
                            if (resp.success)
                            {
                                info.reply = "好的，我记住老师的话了";
                            }
                            else
                            {
                                info.reply = "我的程序有点问题，没记住老师的话555";
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(msg_content))
                            { 
                                info.reply = controller.AnswerOneOneQuestion(user_id, msg_content);
                            }
                        }
                    }
                    else
                    {
                        log.Warn("[Warning]: Received non-text messsage from contact: " + message.user.senderName);
                        info.reply = "对不起，我现在只认识文字。";
                    }                    
                    break;
                case 3: //Group text message
                    //info.toUserId = message.user.groupId;
                    if (!string.IsNullOrWhiteSpace(message.content.detail))
                    {
                        if (message.user.isAtMe)
                        {
                            if (message.content.type == 0)
                            {                                
                                string msg_content = message.content.desc;
                                string src_user_id = message.user.senderId;
                                string src_user_name = message.user.senderName;                                
                                string gid = message.user.groupId;

                                log.Info("[Group AtMe Message]: In Group(" + gid +"), From (" + src_user_name + "), Context: " + msg_content);

                                if (!string.IsNullOrWhiteSpace(msg_content))
                                {
                                    string reply = controller.AnswerQuestion(src_user_id, msg_content);

                                    info.reply = "@" + src_user_name + " " + reply;
                                }
                            }
                            else
                            {
                                log.Warn("[Warning]: Received non-text messsage in group: " + message.user.groupId);
                                info.reply = "对不起，我现在只认识文字。";
                            }
                        }                        
                    }
                    break;
               /* case 100:
                    if (message.content.type == 0)
                    {
                        string msg_content = message.content.data;
                        string user_id = message.user.senderId;
                        string user_name = message.user.senderName;

                        log.Info("[Unknown Contact Message]: From (" + user_name + "), Context: " + msg_content);

                        if (!string.IsNullOrWhiteSpace(msg_content))
                        {
                            info.reply = controller.AnswerOneOneQuestion(user_id, msg_content);

                        }
                    }
                    else
                    {
                        log.Warn("[Warning]: Received non-text messsage from contact: " + message.user.senderName);
                        info.reply = "对不起，我现在只认识文字。";
                    }
                    break;*/
                default:
                    break;
            }
            
            return info;
        }
    }
}

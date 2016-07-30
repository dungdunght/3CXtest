using System;
using System.Text;
using Mono.Options;
using System.ServiceModel;
using Microsoft.Win32;

namespace TCX.CallTriggerCmd
{
    public class Program
    {
        private static string normalize(string input)
        {
            // Strip letters for tel: protocol
            if (input.StartsWith("tel:"))
                input = input.Substring(4);

            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (Char.IsLetter(c))
                {
                    switch (new string(c, 1).ToUpper())
                    {
                        case "A": // fall down
                        case "B": // fall down
                        case "C": sb.Append('2'); break;
                        case "D": // fall down
                        case "E": // fall down
                        case "F": sb.Append('3'); break;
                        case "G": // fall down
                        case "H": // fall down
                        case "I": sb.Append('4'); break;
                        case "J": // fall down
                        case "K": // fall down
                        case "L": sb.Append('5'); break;
                        case "M": // fall down
                        case "N": // fall down
                        case "O": sb.Append('6'); break;
                        case "P": // fall down
                        case "Q": // fall down
                        case "R": // fall down
                        case "S": sb.Append('7'); break;
                        case "T": // fall down
                        case "U": // fall down
                        case "V": sb.Append('8'); break;
                        case "W": // fall down
                        case "X": // fall down
                        case "Y": // fall down
                        case "Z": sb.Append('9'); break;
                    }
                }
                else if (Char.IsDigit(c) || c == '+' || c == '#' || c == '*')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        class ServiceCallback : IClientCallback
        {

            public void CurrentProfileChanged(int profileid)
            {
                Console.WriteLine("Profile changed");
            }

            public void ProfileExtendedStatusChanged(int profileid, string status)
            {
                Console.WriteLine("Extended status changed");
            }

            public void CallStatusChanged()
            {
                Console.WriteLine("Call status changed");
            }

            public void MyPhoneStatusChanged()
            {
                Console.WriteLine("MyPhone status changed");
            }
        };

        static int Main(string[] args)
        {
            try
            {
                bool show_help = false;
                var listen = false;

                var binding = new NetNamedPipeBinding();
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\3CX");
                var uri = key.GetValue("CallTriggerCmdUri");
                if (uri == null)
                    throw new Exception("User specific 3CXPhone CallTrigger uri is not found");

                var address = new EndpointAddress(uri.ToString());
                var channelFactory = new DuplexChannelFactory<ICallTriggerService>(
                    new ServiceCallback(), binding, address);
                var service = channelFactory.CreateChannel();
                service.Subscribe();

                var p = new OptionSet() {
                    { "cmd=:", "Deprecated make call command. Please specify destination as second parameter. Example '-cmd makecall:123'",
                        (string command, string destination) => Console.WriteLine( service.MakeCall(normalize(destination))) },

                    { "c|call=", "Make a call to specified destination",
                        (string destination) => Console.WriteLine( service.MakeCall(normalize(destination))) },

                    { "v|videocall=", "Make a video call to specified destination",
                        (string destination) => Console.WriteLine( service.MakeCallEx(normalize(destination), MakeCallOptions.WithVideo)) },

                    { "d|drop=", "Drop a call with specified call id",
                        (string id) => service.DropCall(id) },

                    { "show=", "Show a specific view",
                        (string id) => {
                            Views result;
                            if (Enum.TryParse<Views>(id, out result))
                                service.Show(result, ShowOptions.None);
                        } },

                    { "hold=", "Hold a call with specified call id",
                        (string id) => service.Hold(id, true) },

                    { "resume=", "Resume a call with specified call id",
                        (string id) => service.Hold(id, false) },

                    { "mute=", "Mute/unmute a call with specified call id",
                        (string id) => service.Mute(id) },

                    { "queue=", "Login/Logout from queue",
                        (int status) => service.SetQueueLoginStatus(status > 0) },

                    { "dtmf=/", "Send DTMF codes to specific call id",
                        (string selectedCallId, string dtmf) => service.SendDTMF(selectedCallId, dtmf) },

                    { "listen", "Listen to events after finishing jobs",
                        (l) => listen = true },

                    { "activate=", "Activate a call with specified call id",
                        (string id) => service.Activate(id) },

                    { "vactivate=", "Activate a call with specified call id with video",
                        (string id) => service.ActivateEx(id, ActivateOptions.WithVideo) },

                    { "h|help", "Show this help",
                        v => show_help = v != null},

                    { "b|blind=/", "Blind transfer selected call to destination",
                        (string selectedCallId, string destination) => service.BlindTransfer(selectedCallId, normalize(destination))},

                    { "a|attended=/", "Begin attended transfer selected call to destination",
                        (string selectedCallId, string destination) => Console.WriteLine( service.BeginTransfer(selectedCallId, normalize(destination)) )},

                    { "cancel=", "Stop attended transfer for call id",
                        (string id) => service.CancelTransfer(id)},

                    { "t|transfer=", "Finalize attended transfer for call id",
                        (string id) => service.CompleteTransfer(id)},

                    { "p|profiles", "Show list of all available user profiles",
                        x =>
                            {
                                foreach (var profile in service.Profiles)
                                    Console.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}",
                                        profile.ProfileId, profile.Name, profile.CustomName, profile.ExtendedStatus));
                            }},

                    { "set-active-profile=", "Set active profile",
                        (int profileId) =>
                            {
                                service.SetActiveProfile(profileId);
                            }},

                    { "set-profile-status=/", "Set profile status",
                        (int profileId, string status) =>
                            {
                                service.SetProfileExtendedStatus(profileId, status);
                            }},

                    { "l|list", "Show list of active calls", l =>
                        {
                            foreach( var id in service.ActiveCalls )
                                Console.WriteLine("{0} {1}", id.CallID, id.State);
                        }
                    },
                };
                p.Parse(args);
                if ( show_help )
                    p.WriteOptionDescriptions(Console.Out);
                if ( listen )
                    Console.ReadLine();
                return 0;
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return -1;
            }
        }
    }
}

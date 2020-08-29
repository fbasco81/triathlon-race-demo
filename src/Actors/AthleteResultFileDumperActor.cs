using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Akka.Streams.Dsl;

namespace Actors
{
    public class AthleteResultFileDumperActor : ReceiveActor
    {
        private string _filePath;

        public AthleteResultFileDumperActor(string filePath)
        {
            _filePath = filePath;

            Func<TimeSpan, string> dumpDuration = (TimeSpan ts) => { return ts.TotalMilliseconds.ToString(); };
            Func<DateTime, string> dumpDuTimestamp = (DateTime dt) => { return dt.ToString("HH:mm:ss.ffffff"); };
            Receive<AthleteRaceResult>( msg => {
                var entries = new List<string>();
                entries.Add(msg.BibId);
                foreach(var kv in msg.Gates)
                {
                    var isT = (kv.Key == "T1" || kv.Key == "T2");
                    entries.Add(kv.Value == null ? "" : dumpDuration(kv.Value.Duration.Value));
                    if(!isT)
                    {
                        entries.Add(kv.Value==null? "" : dumpDuTimestamp(kv.Value.In));
                        entries.Add((kv.Value==null || kv.Value.Out.Value==null)? "" : dumpDuTimestamp(kv.Value.Out.Value));
                    }
                }

                //FileIO.ToFile(new FileInfo(_filePath)).

                //Console.WriteLine(String.Join("|", entries));
                File.AppendAllText(
                    _filePath,
                    String.Join("|", entries) + System.Environment.NewLine
                );
            });
        }


    }
}

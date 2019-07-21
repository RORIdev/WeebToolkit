using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnimeOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 1)
            {
                DirectoryInfo dir = new DirectoryInfo(args[0]);
                var listOfFiles = dir.EnumerateFiles().ToList();
                if (listOfFiles.Any(x => x.Name == "schema.txt"))
                {
                    string[] schemaTxt = File.ReadAllLines(dir.FullName + "/schema.txt");
                    config cfg = Parse(schemaTxt);
                    if (!string.IsNullOrEmpty(cfg.episodeLocale))
                    {
                        Console.WriteLine($"Locale : {cfg.episodeLocale} | Subtitle.{cfg.subtitleExtension} | Output :{cfg.output}");
                        Organize(dir, cfg);
                    }
                    else
                    {
                        Console.WriteLine("Invalid Schema. Did you forget to remove the comment symbol (#)?");
                    }
                }
                else
                {
                    GenerateBasicSchema(dir);
                    Console.WriteLine("No schema was detected. A new schema.txt file was created.\nFollow the instructions on the file and run the program again.");
                }
            }
            else
            {
                Console.WriteLine("Correct Usage : AnimeOrganizer folder");
            }
        }

        private static void Organize(DirectoryInfo dir, config cfg)
        {
            var listOfFiles = dir.EnumerateFiles().ToList();
            foreach (var file in listOfFiles)
            {
                if (file.Name.Contains(".mp4") || file.Name.Contains(cfg.subtitleExtension)) 
                {
                    string name = file.Name;
                    Regex dubFinder = new Regex(@"(\(\w+\)\s)");
                    var find = dubFinder.Matches(name);
                    if (find.Count > 0)
                    {
                        for (int i = 0; i < find.Count; i++)
                        {
                            var f = find[i];
                            name = name.Replace(f.Value, "");
                        }
                    }
                    List<string> decode = cfg.genericFormat.Split(' ').ToList();
                    episode ep = new episode();
                    List<string> words = name.Split(' ').ToList();
                    int pos = words.IndexOf(cfg.episodeLocale);
                    if(pos == -1)
                    {
                        pos = words.IndexOf("MOEDL");
                    }
                    string buffer = "";
                    if (decode[decode.IndexOf(cfg.episodeLocale) - 1] != null && decode[decode.IndexOf(cfg.episodeLocale) - 1] == "n")
                    {
                        for (int i = 0; i < pos; i++)
                        {
                            buffer += $"{words[i]} ";
                        }

                        ep.parentName = buffer.Remove(buffer.Length - 1, 1); ;
                    }
                    if (decode[decode.IndexOf(cfg.episodeLocale) + 1] != null && decode[decode.IndexOf(cfg.episodeLocale) + 1] == "x")
                    {
                        ep.episodeNumber = int.Parse(words[pos + 1]);
                    }

                    string dair = "";
                    dair += file.DirectoryName;
                    dair += "/";
                    foreach (var s in cfg.output)
                    {

                        if (s == '/')
                        {
                            dair += "/";
                        }
                        else if (s == 'x')
                        {
                            dair += ep.episodeNumber;
                        }
                        else if (s == 'n')
                        {
                            dair += ep.parentName;
                        }
                        else
                        {
                            dair += s;
                        }
                    }
                    dair += file.Extension;
                    var directory = dair.Split('/').ToList();
                    directory.Remove(directory.Last());
                    string ir = "";
                    directory.ForEach(x => ir += $"{x}/");
                    if (dir.EnumerateDirectories().ToList().Any(x => x.Name == ep.parentName))
                    {

                        file.MoveTo(dair);
                        Console.WriteLine($"[{ep.parentName}] Episode #{ep.episodeNumber} moved to /{ep.parentName}");
                    }
                    else
                    {

                        dir.CreateSubdirectory(ep.parentName);
                        file.MoveTo(dair);
                        Console.WriteLine($"[{ep.parentName}] Episode #{ep.episodeNumber} moved to /{ep.parentName}");
                    }


                }


            }
            Console.WriteLine($"AnimeOrganize Finished.");
        }

        private static config Parse(string[] schemaTxt)
        {
            List<string> allCodex = new List<string>();
            List<string> validTypes = new List<string>{
            "n","x","d","s","-", "d-id.s"
            };
            config c = new config();
            c.output = "n/Ex";
            c.subtitleExtension = "ass";
            for (int i = 0; i < schemaTxt.Count(); i++)
            {
                if (!String.IsNullOrEmpty(schemaTxt[i]) && schemaTxt[i][0] != '#')
                {
                    allCodex.Add(schemaTxt[i]);
                }
            }
            foreach (var ln in allCodex)
            {
                if (ln[0] == 'n')
                {
                    c.genericFormat = ln;
                    var innerArg = ln.Split(' ').ToList();
                    foreach (var arg in innerArg)
                    {
                        if (!validTypes.Contains(arg))
                        {
                            if (arg.Count() > 1)
                            {
                                c.episodeLocale = arg;
                            }
                        }
                    }
                }
                if (ln.StartsWith("o>"))
                {

                    c.output = ln.Remove(0, 2);
                }
                if (ln.StartsWith("s:"))
                {

                    c.subtitleExtension = ln.Remove(0, 2);
                }
            }
            return c;
        }

        private static void GenerateBasicSchema(DirectoryInfo dir)
        {
            string[] lines = { "# schema parser", "# n - name of series x - episode number d - description s - subtitle BLANK - how \"episode\" is spelled on the output.", "# ex : Eromanga Sensei Episódio 1 – A irmã mais nova e uma porta que nunca se abre-734155.ptBR -> n Episódio x - d-id.s", "#o> Output [Valid Types : x , / <- Creates Directory] : default \"n/Ex\"", "#s: Subtitle extension : default \"ass\"", "", "#n BLANK x – d-id.s", "#s:ass", "#o>n/Ex" };
            File.WriteAllLines(dir.FullName + "/schema.txt", lines);
        }
    }

    class episode
    {
        public string parentName { get; set; }
        public string description { get; set; }
        public int episodeNumber { get; set; }
    }
    class config
    {
        public string genericFormat { get; set; }
        public string subtitleExtension { get; set; }
        public string episodeLocale { get; set; }
        public string output { get; set; }
    }
}

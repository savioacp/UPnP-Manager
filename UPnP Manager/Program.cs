using System;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NATUPNPLib;
using System.Net;

namespace UPnP_Manager
{
    class Program
    {
        static Regex addCommandRegex;
        static Regex delCommandRegex;
        static UPnPNATClass nat;
        static void Main(string[] args)
        {
            addCommandRegex = new Regex(@"add (\d{1,5}) (\d{1,5}) (tcp|udp) (.+)$", RegexOptions.Compiled & RegexOptions.Singleline);
            delCommandRegex = new Regex(@"del (\d{1,5})$", RegexOptions.Compiled & RegexOptions.Singleline);
            nat = new UPnPNATClass();
            ForegroundColor = ConsoleColor.Yellow;
            WriteLine("NAT UPnP Entries:");
            List<IStaticPortMapping> natList = new List<IStaticPortMapping>();
            ResetColor();
            try
            {
                int j = 0;
                foreach (IStaticPortMapping i in nat.StaticPortMappingCollection)
                {
                    j++;
                    WriteLine(string.Format("{4}) {0,-3} | {1}:{2,-5} | {3}", i.Protocol, i.InternalClient, i.InternalPort, i.Description, j));
                    natList.Add(i);
                }
                ReadKey();
            }
            catch (Exception e) { }
            while (true)
            {
                Write("\nnat >");
                string cmd = ReadLine().ToLowerInvariant();
                if (cmd == "exit")
                    Environment.Exit(0);
                if (cmd == "help")
                {
                    Write(string.Format("{0,-30} - Mostra este texto.\n", "help") +
                    string.Format("{0,-30} - Lista as entradas\n", "list") +
                    string.Format("{0,-30} - Adiciona uma entrada\n", "add <inport> <extport> <proto> <desc>") +
                    string.Format("{0,-30} - Deleta uma entrada\n", "del <i>") +
                    string.Format("{0,-30} - Edita uma entrada\n", "edit <i>") +
                    string.Format("{0,-30} - Sai do programa\n", "exit"));
                }

                

                if (cmd == "list")
                try
                {
                    int j = 0;
                    foreach (IStaticPortMapping i in nat.StaticPortMappingCollection)
                    {
                        j++;
                        WriteLine(string.Format("{4}) {0,-3} | {1}:{2,-5} | {3}", i.Protocol, i.InternalClient, i.InternalPort, i.Description, j));
                        
                    }
                        ReadKey();
                }
                catch (Exception e) { }

                if (cmd.StartsWith("add"))
                {
                    
                    if (!addCommandRegex.IsMatch(cmd))
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("[-] Erro: sintaxe errada.");
                        ResetColor();
                        WriteLine("Uso: add <internalPort> <externalPort> <protocol> <description>");
                        continue;
                    }
                    var match = addCommandRegex.Match(cmd);
                    var internalPort = ushort.Parse(match.Groups[1].Value);
                    var externalPort = ushort.Parse(match.Groups[2].Value);
                    var protocol = match.Groups[3].Value.ToUpper();
                    var description = match.Groups[4].Value;

                    if (0 < internalPort && internalPort <= 0xFFFF) { }
                    else
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("[-] Erro: internalPort deve estar entre 1 e 65535.");
                        ResetColor();
                        continue;
                    }

                    if (0 < externalPort && externalPort <= 0xFFFF) { }
                    else
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("[-] Erro: externalPort deve estar entre 1 e 65535.");
                        ResetColor();
                        continue;
                    }

                    nat.StaticPortMappingCollection.Add(externalPort,
                        protocol.ToUpper(),
                        internalPort,
                        Dns.GetHostEntry(
                            Dns.GetHostName())
                                .AddressList
                                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString(),
                        true,
                        description);

                }

                if (cmd.StartsWith("del"))
                {
                    if (!delCommandRegex.IsMatch(cmd))
                    {
                        ForegroundColor = ConsoleColor.Red;
                        WriteLine("[-] Erro: sintaxe errada.");
                        ResetColor();
                        WriteLine("Uso: del <index>");
                        continue;
                    }

                    ushort index = ushort.Parse(delCommandRegex.Match(cmd).Groups[1].Value);

                    nat.StaticPortMappingCollection.Remove(natList[index - 1].ExternalPort, natList[index - 1].Protocol);
                    natList.RemoveAt(index - 1);
                }
            }
        }           
    }
}
            
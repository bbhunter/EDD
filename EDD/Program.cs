﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Options;

namespace EDD
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // parse CLI
                string functionName = null;
                string fileSavePath = null;
                string targetedGroupName = null;
                string computerTarget = null;
                string processTargeted = null;
                string userAccountTargeted = null;
                bool show_help = false;
                var p = new OptionSet() {
                    { "f|function=", "the function you want to use", (v) => functionName = v },
                    { "o|output=", "the path to the file to save", (v) => fileSavePath = v },
                    { "c|computer=", "the computer you are targeting", (v) => computerTarget = v },
                    { "g|groupname=", "the domain group you are targeting", (v) => targetedGroupName = v },
                    { "p|processname=", "the process you are targeting", (v) => processTargeted = v },
                    { "u|username=", "the domain account you are targeting", (v) => userAccountTargeted = v },
                    { "h|help",  "show this message and exit",
                        v => show_help = v != null },
                };

                List<string> cliArgs;
                try
                {
                    cliArgs = p.Parse(args);
                }
                catch (OptionException e)
                {
                    Console.Write("EDD.exe: ");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Try `EDD.exe --help' for more information.");
                    return;
                }

                if (show_help)
                {
                    ShowHelp(p);
                    return;
                }


                switch (functionName.ToLower())
                {
                    case "getdomaincomputers":
                        LDAP domainQuery = new LDAP();
                        List<string> domainComputers = domainQuery.CaptureComputers();
                        if (domainComputers.Count > 0)
                        {
                            if (fileSavePath == null)
                            {
                                foreach (string systemName in domainComputers)
                                {
                                    Console.WriteLine(systemName);
                                }
                            }
                            else
                            {
                                using (TextWriter tw = new StreamWriter(fileSavePath))
                                {
                                    foreach (String s in domainComputers)
                                        tw.WriteLine(s);
                                }
                            }
                        }
                        break;

                    case "getdomaincontrollers":
                        LDAP findDCs = new LDAP();
                        List<string> domainControllers = findDCs.CaptureDomainControllers();
                        if (domainControllers.Count > 0)
                        {
                            if (fileSavePath == null)
                            {
                                foreach (string systemName in domainControllers)
                                {
                                    Console.WriteLine(systemName);
                                }
                            }
                            else
                            {
                                using (TextWriter tw = new StreamWriter(fileSavePath))
                                {
                                    foreach (String s in domainControllers)
                                        tw.WriteLine(s);
                                }
                            }
                        }
                        break;

                    case "getnetlocalgroupmember":
                        
                        if ((computerTarget != null) & (targetedGroupName != null))
                        {
                            Amass shepherd = new Amass();
                            List<string> localGroupMembers = shepherd.GetLocalGroupMembers(computerTarget, targetedGroupName);
                            if (localGroupMembers.Count > 0)
                            {
                                if (fileSavePath == null)
                                {
                                    foreach (string groupMember in localGroupMembers)
                                    {
                                        Console.WriteLine(groupMember);
                                    }
                                }
                                else
                                {
                                    using (TextWriter tw = new StreamWriter(fileSavePath))
                                    {
                                        foreach (String s in localGroupMembers)
                                            tw.WriteLine(s);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error:");
                            Console.WriteLine("You need to provide both the system and domain group you are targeting");
                            Console.WriteLine("Please re-run with the required options");
                            Console.WriteLine("Exiting...");
                        }
                        break;

                    case "getdomaingroupmember":
                        if (targetedGroupName != null)
                        {
                            Amass groupMemberEnum = new Amass();
                            List<string> groupMembers = groupMemberEnum.GetDomainGroupMembers(targetedGroupName);
                            if (groupMembers.Count > 0)
                            {
                                if (fileSavePath == null)
                                {
                                    foreach (string groupMember in groupMembers)
                                    {
                                        Console.WriteLine(groupMember);
                                    }
                                }
                                else
                                {
                                    using (TextWriter tw = new StreamWriter(fileSavePath))
                                    {
                                        foreach (String s in groupMembers)
                                            tw.WriteLine(s);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("You did not provide a group name to target");
                            Console.WriteLine("Exiting...");
                        }
                        break;

                    case "getnetsession":
                        if (computerTarget != null)
                        {
                            Amass sessionInfo = new Amass();
                            List<Amass.SESSION_INFO_10> incomingSessions = sessionInfo.GetRemoteSessionInfo(computerTarget);
                            if (incomingSessions.Count > 0)
                            {
                                if (fileSavePath == null)
                                {
                                    foreach (Amass.SESSION_INFO_10 sessionInformation in incomingSessions)
                                    {
                                        Console.WriteLine("Connection From: " + sessionInformation.sesi10_cname);
                                        Console.WriteLine("Idle Time: " + sessionInformation.sesi10_idle_time);
                                        Console.WriteLine("Total Active Time: " + sessionInformation.sesi10_time);
                                        Console.WriteLine("Username: " + sessionInformation.sesi10_username);
                                    }
                                }
                                else
                                {
                                    using (TextWriter tw = new StreamWriter(fileSavePath))
                                    {
                                        foreach (Amass.SESSION_INFO_10 sessIn in incomingSessions)
                                        {
                                            tw.WriteLine("Connection From: " + sessIn.sesi10_cname);
                                            tw.WriteLine("Idle Time: " + sessIn.sesi10_idle_time);
                                            tw.WriteLine("Total Active Time: " + sessIn.sesi10_time);
                                            tw.WriteLine("Username: " + sessIn.sesi10_username);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("You need to specify a computer to target!");
                            Console.WriteLine("Exiting...");
                        }
                        
                        break;

                    case "getnetloggedon":
                        if (computerTarget != null)
                        {
                            Amass loggedInInfo = new Amass();
                            List <Amass.WKSTA_USER_INFO_1> loggedInAccounts = loggedInInfo.GetLoggedOnUsers(computerTarget);
                            if (loggedInAccounts.Count > 0)
                            {
                                if (fileSavePath == null)
                                {
                                    foreach (Amass.WKSTA_USER_INFO_1 sessionInformation in loggedInAccounts)
                                    {
                                        Console.WriteLine("Account Name: " + sessionInformation.wkui1_username);
                                        Console.WriteLine("Domain Used by Account: " + sessionInformation.wkui1_logon_domain);
                                        Console.WriteLine("Operating System Domains: " + sessionInformation.wkui1_oth_domains);
                                        Console.WriteLine("Logon server: " + sessionInformation.wkui1_logon_server);
                                    }
                                }
                                else
                                {
                                    using (TextWriter tw = new StreamWriter(fileSavePath))
                                    {
                                        foreach (Amass.WKSTA_USER_INFO_1 sessIn in loggedInAccounts)
                                        {
                                            tw.WriteLine("Account Name: " + sessIn.wkui1_username);
                                            tw.WriteLine("Domain Used by Account: " + sessIn.wkui1_logon_domain);
                                            tw.WriteLine("Operating System Domains: " + sessIn.wkui1_oth_domains);
                                            tw.WriteLine("Logon server: " + sessIn.wkui1_logon_server);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("You need to provide a computer to target!");
                            Console.WriteLine("Exiting...");
                        }

                        break;

                    case "finddomainuser":
                        if (targetedGroupName == null && userAccountTargeted == null)
                        {
                            LDAP compQuery = new LDAP();
                            List<string> windowsComputers = compQuery.CaptureComputers();
                            Amass findUser = new Amass();
                            List<string> targetedUserList = new List<string>();
                            List<Amass.SESSION_INFO_10> accountSessionList = new List<Amass.SESSION_INFO_10>();
                            List<Amass.WKSTA_USER_INFO_1> loggedInUsers = new List<Amass.WKSTA_USER_INFO_1>();
                            List<string> domainAdminList = findUser.GetDomainGroupMembers("Domain Admins");
                            if (windowsComputers.Count > 0)
                            {
                                foreach (string computerHostName in windowsComputers)
                                {
                                    
                                }
                            }
                        }
                        break;

                    case "getdomainusers":
                        Amass userInfo = new Amass();
                        List<string> allDomainUsers = userInfo.GetDomainUsersInfo();
                        if (allDomainUsers.Count > 0)
                        {
                            if (fileSavePath == null)
                            {
                                foreach (string userName in allDomainUsers)
                                {
                                    Console.WriteLine(userName);
                                }
                            }
                            else
                            {
                                using (TextWriter tw = new StreamWriter(fileSavePath))
                                {
                                    foreach (String s in allDomainUsers)
                                        tw.WriteLine(s);
                                }
                            }
                        }
                        break;

                    case "getdomainuser":
                        if (userAccountTargeted != null)
                        {
                            Amass singleUserInfo = new Amass();
                            UserObject soleUser = singleUserInfo.GetDomainUserInfo(userAccountTargeted);
                            Console.WriteLine("SamAccountName: " + soleUser.SamAccountName);
                            Console.WriteLine("Name: " + soleUser.Name);
                            Console.WriteLine("Description: " + soleUser.Description);
                            Console.WriteLine("Distinguished Name: " + soleUser.DistinguishedName);
                            Console.WriteLine("SID: " + soleUser.SID);
                            Console.WriteLine("\nDomain Groups:");
                            foreach (string singleGroupName in soleUser.DomainGroups)
                            {
                                Console.WriteLine(singleGroupName);
                            }
                        }
                        else
                        {
                            Console.WriteLine("You did not provide a user account to look up!");
                            Console.WriteLine("Exiting...");
                        }
                        break;

                    case "getdomainshares":
                        LDAP computerQuery = new LDAP();
                        List<string> domainSystems = computerQuery.CaptureComputers();
                        Amass shareMe = new Amass();
                        List<string> allShares = shareMe.GetShares(domainSystems);
                        if (allShares.Count > 0)
                        {
                            if (fileSavePath == null)
                            {
                                foreach (string groupMember in allShares)
                                {
                                    Console.WriteLine(groupMember);
                                }
                            }
                            else
                            {
                                using (TextWriter tw = new StreamWriter(fileSavePath))
                                {
                                    foreach (String s in allShares)
                                        tw.WriteLine(s);
                                }
                            }
                        }
                        break;

                    case "finddomainprocess":
                        if (processTargeted != null)
                        {
                            LDAP procQuery = new LDAP();
                            List<string> procComputers = procQuery.CaptureComputers();
                            WMI processSearcher = new WMI();
                            List<string> systemsWithProc = processSearcher.CheckProcesses(procComputers, processTargeted);
                            if (systemsWithProc.Count > 0)
                            {
                                if (fileSavePath == null)
                                {
                                    foreach (string singleSys in systemsWithProc)
                                    {
                                        Console.WriteLine(singleSys);
                                    }
                                }
                                else
                                {
                                    using (TextWriter tw = new StreamWriter(fileSavePath))
                                    {
                                        foreach (String s in systemsWithProc)
                                            tw.WriteLine(s);
                                    }
                                }
                            }
                        }
                        
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error is - " + e);
            }

            Console.WriteLine("\n[!] EDD is done running!");
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: EDD.exe -f <function name> -<extra options>");
            Console.WriteLine("Provide the function you want to run to enumerate that data from the domain");
            Console.WriteLine("Also provide any other extra options that you need for the specific function");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

using CommandLine;
using Hokanson.JenkinsClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JenkinsJobSubmitter
{
    class Program
    {
        static void Main(string[] args) => Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ToString());
                    }
                })
                .WithParsed(options =>
                {
                    try
                    {
                        using (var jenkinsClient = new JenkinsClient(new JenkinsConfiguration()))
                        {
                            if (options.Parameters.Count() == 0)
                            {
                                jenkinsClient.SubmitJobAsync(options.JobName)
                                             .Wait();
                            }
                            else
                            {
                                IEnumerable<(string, string)> parameters = options.Parameters.Select(p =>
                                {
                                    string[] parts = p.Split('=');
                                    if (parts.Length != 2) throw new Exception("parameter name-value pair must be separated by an equals sign");
                                    return (parts[0], parts[1]);
                                });

                                jenkinsClient.SubmitParameterizedJob(options.JobName, parameters)
                                             .Wait();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        do
                        {
                            Console.WriteLine(e.Message);
                            e = e.InnerException;
                        }
                        while (e != null);
                    }
                });
    }

    class Options
    {
        [Option('j', "jobname", Required = true, HelpText = "The name of the job to be submitted")]
        public string JobName { get; set; }

        [Option('p', "parameterlist", HelpText = "List of equals-separated name-value pairs of parameters for the requested job")]
        public IEnumerable<string> Parameters { get; set; }
    }
}
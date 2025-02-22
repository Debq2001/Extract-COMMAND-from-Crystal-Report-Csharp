﻿using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.DataDefModel;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using CrystalDecisions.ReportAppServer;
using System.Reflection;
using CrystalDecisions.ReportAppServer.Controllers;
using System.Web.UI.WebControls;
using System.Linq.Expressions;
using System.Diagnostics.Eventing.Reader;

namespace CR_CMD_EXT_TO_FILE
{
    public partial class ExtractCMDFILE : Form
    {
        public ExtractCMDFILE()
        {
            InitializeComponent();
        }

        //Button to Navigate to browse to Directory path folder.
       
        private void button1_Click_1(object sender, EventArgs e)

        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        //Button to Navigate to Save File Location path
        private void button2_Click_1(object sender, EventArgs e)

        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        // Create a progress bar.
        private Task ProcessData(List<string> list, IProgress<ProgressReport> progressBar1)
        {
            int index = 1;
            int totalProcess = list.Count;
            var progressReport = new ProgressReport();
            return Task.Run(() =>
            {
                for (int i = 0; i < totalProcess; i++)
                {
                    progressReport.PercentComplete = index++ * 100 / totalProcess;
                    progressBar1.Report(progressReport);
                    Thread.Sleep(10);//used to simulate lenght of operation
                }
            });
        }

        //Button to begin interation through files in the selected Directory
        private async void button4_Click_1(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 1000; i++)
                list.Add(i.ToString());
            label2.Text = "Working...";
            var progress = new Progress<ProgressReport>();
            progress.ProgressChanged += (o, report) =>
            {
                label2.Text = string.Format("Processing...{0}%", report.PercentComplete);
                progressBar1.Value = report.PercentComplete;
                progressBar1.Update();

            };

            //For each file in Directory find .rpt. Get Files with the .rpt extension -> Load the file into Crystal Reports Engine
            CrystalDecisions.CrystalReports.Engine.ReportDocument doc = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
            foreach (string files in Directory.GetFiles(textBox1.Text, "*.rpt", SearchOption.AllDirectories))
            {
                try
                {
                    Console.WriteLine(String.Format("Processing {0}...", files));
                    doc.Load(files);
                }
                catch (CrystalReportsException ex)
                {
                    //Save the exception to a Log file
                    string error = ex.ToString();
                    string exfilepath = @"3 - ErrorLog\\log.txt";

                    //string FilePath = Path.Combine(savepath + "\\" + filename) + ".sql";
                    //Append the error details to the text file
                    File.AppendAllText(exfilepath, error);

                }
                {
                    {
                        try
                        {
                            {
                                //For each file with .rpt find the file with ClassName = CrystalReports. CommandTable
                                //MAIN REPORT = COMMAND - no SUBREPORT  
                                foreach (dynamic mntable in doc.ReportClientDocument.DatabaseController.Database.Tables)
                                {

                                    if (mntable.ClassName == "CrystalReports.CommandTable")
                                    {
                                        ISCDReportClientDocument mnrptClientDoc;
                                        mnrptClientDoc = doc.ReportClientDocument;

                                        foreach (CrystalDecisions.ReportAppServer.DataDefModel.Table mntmpTbl in mnrptClientDoc.DatabaseController.Database.Tables)
                                            if (mntmpTbl is CommandTable)
                                            {
                                                CommandTable mncmdTbl = (CommandTable)mntmpTbl;
                                                string mncommandText = mncmdTbl.CommandText;
                                            }
                                        //For each file with .rpt find the file with ClassName = CrystalReports. CommandTable
                                        //MAIN REPORT = COMMAND - SUBREPORT = COMMAND               
                                        //For each file with .rpt find the file with ClassName = CrystalReports. CommandTable

                                        foreach (dynamic Subtable in doc.ReportClientDocument.DatabaseController.Database.Tables)
                                        {
                                            if (!doc.IsSubreport && Subtable.ClassName == "CrystalReports.CommandTable")
                                            {

                                                ISCDReportClientDocument rptSubClientDoc;
                                                rptSubClientDoc = doc.ReportClientDocument;

                                                foreach (dynamic subName in rptSubClientDoc.SubreportController.GetSubreportNames())
                                                    foreach (CrystalDecisions.ReportAppServer.DataDefModel.Table tmpSubTbl in rptSubClientDoc.DatabaseController.Database.Tables)

                                                        if (tmpSubTbl is CommandTable)
                                                        {
                                                            CommandTable cmdSubTbl = (CommandTable)tmpSubTbl;
                                                            string commandSub = cmdSubTbl.CommandText;
                                                        }
                                                //Create sql from CommandText
                                                string mncommandtext = mntable.CommandText;
                                                string commandsub = Subtable.CommandText;

                                                //Save the SQL command to a file
                                                string srcfilePath = doc.FileName;
                                                string filename = Path.GetFileNameWithoutExtension(srcfilePath);

                                                //Save the file to the Save File Path
                                                string savepath = textBox2.Text;
                                                string FilePath = Path.Combine(savepath + "\\" + filename) + ".sql";

                                                File.WriteAllText(FilePath, mncommandtext + commandsub);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        catch (CrystalReportsException ex)
                        {
                            //Save the exception to a Log file
                            string error = ex.ToString();
                            string exfilepath = @"3 - ErrorLog\\log.txt";

                            //string FilePath = Path.Combine(savepath + "\\" + filename) + ".sql";
                            //Append the error details to the text file
                            File.AppendAllText(exfilepath, error);

                        }
                        try
                        {

                            CrystalDecisions.CrystalReports.Engine.ReportObjects crReportObjects;
                            CrystalDecisions.CrystalReports.Engine.SubreportObject crSubreportObject;
                            CrystalDecisions.CrystalReports.Engine.ReportDocument crSubreportDocument;

                            ISCDReportClientDocument rptSubsubClientDoc;
                            rptSubsubClientDoc = doc.ReportClientDocument;
                            try
                            {
                                //set the crSections object to the current report's sections
                                CrystalDecisions.CrystalReports.Engine.Sections crSections = doc.ReportDefinition.Sections;

                                //loop through all the sections to find all the report objects
                                foreach (CrystalDecisions.CrystalReports.Engine.Section crSection in crSections)
                                {
                                    crReportObjects = crSection.ReportObjects;
                                    //loop through all the report objects to find all the subreports
                                    foreach (CrystalDecisions.CrystalReports.Engine.ReportObject crReportObject in crReportObjects)
                                    {
                                        if (crReportObject.Kind == ReportObjectKind.SubreportObject)
                                        {
                                            //you will need to typecast the reportobject to a subreport 
                                            //object once you find it
                                            crSubreportObject = (CrystalDecisions.CrystalReports.Engine.SubreportObject)crReportObject;
                                            crSubreportDocument = crSubreportObject.OpenSubreport(crSubreportObject.SubreportName);
                                            SubreportClientDocument subRCD = rptSubsubClientDoc.SubreportController.GetSubreport(crSubreportObject.SubreportName);
                                            string mysubname = crSubreportObject.SubreportName.ToString();

                                            //subRCD = crSubreportObject.OpenSubreport(crSubreportObject.SubreportName);

                                            if (subRCD.DatabaseController.Database.Tables.Count != 0)
                                            {
                                                foreach (CrystalDecisions.ReportAppServer.DataDefModel.Table crTable in subRCD.DatabaseController.Database.Tables)
                                                {
                                                    try
                                                    {
                                                        // Subreport is using a Command so use RAS to get the SQL
                                                        if (((dynamic)crTable.Name) == "Command")
                                                        {
                                                            CrystalDecisions.ReportAppServer.Controllers.DatabaseController databaseController = subRCD.DatabaseController;
                                                            CommandTable SuboldTable = (CommandTable)databaseController.Database.Tables[0];
                                                            ((dynamic)SuboldTable).CommandText.ToString();

                                                            string commandSubsub = SuboldTable.CommandText;

                                                            //Save the SQL command to a file
                                                            string SubsrcfilePath = doc.FileName;
                                                            string Subfilename = Path.GetFileNameWithoutExtension(SubsrcfilePath);

                                                            //Save the file to the Save File Path
                                                            string Subsavepath = textBox2.Text;
                                                            string SubFilePath = Path.Combine(Subsavepath + "\\" + Subfilename) + "-sub" + ".sql";

                                                            File.WriteAllText(SubFilePath, commandSubsub);
                                                        }
                                                    }
                                                    catch (InvalidCastException ex)
                                                    {
                                                        //Save the exception to a Log file
                                                        string error = ex.ToString();
                                                        string exfilepath = @"3 - ErrorLog\\log.txt";

                                                        //string FilePath = Path.Combine(savepath + "\\" + filename) + ".sql";
                                                        //Append the error details to the text file
                                                        File.AppendAllText(exfilepath, error);

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (CrystalReportsException ex)
                            {
                                //Save the exception to a Log file
                                string error = ex.ToString();
                                string exfilepath = @"3 - ErrorLog\\log.txt";

                                //string FilePath = Path.Combine(savepath + "\\" + filename) + ".sql";
                                //Append the error details to the text file
                                File.AppendAllText(exfilepath, error);

                            }

                        }
                        catch (LoadSaveReportException ex)
                        {
                            //Save the exception to a Log file
                            string error = ex.ToString();
                            string exfilepath = @"3 - ErrorLog\\log.txt";

                            //string FilePath = Path.Combine(savepath + "\\" + filename) + ".sql";
                            //Append the error details to the text file
                            File.AppendAllText(exfilepath, error);
                        }
                    }
                }
            }
                
            
            //Begin the Green progress bar - When ALL files in the chosen folder are processed show "Done"
            await ProcessData(list, progress);
            label2.Text = "Done !";
        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
 }



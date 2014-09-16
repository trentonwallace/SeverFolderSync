using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace WebFolderSync
{
    public partial class frmMain : Form
    {
        
        List<PathEntity> allPathEntity = new List<PathEntity>();
        private const string SERVMAIL_WEB_FOLDER = @"c:\servers\servmail\web$";
        private const string SERVER_DB_WEB_FOLDER = @"c:\servers\server_db\common$\web";
        private const string SERVERDB_WEB_FOLDER = @"c:\servers\serverdb\common\web";

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Show();

            SyncFiles();

            Environment.Exit(0);
        }

        private void SetStatus(string msgText)
        {
            lblStatus.Text = msgText.Trim();
            listBox1.Items.Add(msgText.Trim());
            Application.DoEvents();
        }

        private void SyncFiles()
        {
            try
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 4;
                progressBar1.Value = 0;
                Application.DoEvents();

                //need to load first so allPathEntity is populated
                SetStatus("Loading Folder Paths...");
                LoadPathEntities();
                progressBar1.Value++;

                SetStatus("Updating Files...");
                UpdateFiles();
                progressBar1.Value++;

                SetStatus("Deleting Files...");
                DeleteFiles();
                progressBar1.Value++;

                SetStatus("Adding Files...");
                AddFiles();
                progressBar1.Value++;
            }
            catch (Exception ex)
            {
             LogError.Log("Class:WebFolderSync;SyncFiles()" + "No SQL" +ex.ToString());
            }

            SetStatus("Completed");

            this.Close();
        }
        
        private void UpdateFiles()
        {
            DateTime modDate1 = new DateTime();
            DateTime modDate2 = new DateTime();
            DateTime modDate3 = new DateTime();
            DateTime compareDate = new DateTime();

            string fullPath1 = "";
            string fullPath2 = "";
            string fullPath3 = "";
            string compareFullPath = "";
            string path1 = "";
            string path2 = "";
            string path3 = "";
            string comparePath = "";

            int ct = 0;

            try
            {
                //go through every path entity
                foreach (PathEntity pathEntity in allPathEntity)
                {
                    //if there is three copies of the file check if they are the same date
                    //if not copy the one with the newest date to the other two
                    
                    if (pathEntity.Count == 3)
                    {
                        //every three entities should be added to the temp variables
                        //so that they can be evaluated

                        if (ct == 2)
                        {
                            modDate3 = pathEntity.ModifiedDate;
                            fullPath3 = pathEntity.FullPath;
                            path3 = pathEntity.ComparePath;
                        }
                        if (ct == 1)
                        {
                            modDate2 = pathEntity.ModifiedDate;
                            fullPath2 = pathEntity.FullPath;
                            path2 = pathEntity.ComparePath;
                        }
                        if (ct == 0)
                        {
                            modDate1 = pathEntity.ModifiedDate;
                            fullPath1 = pathEntity.FullPath;
                            path1 = pathEntity.ComparePath;
                        }
                        ct++;

                        if (ct == 3)
                        {
                            //if they aren't equal
                            if (modDate1.CompareTo(modDate2) != 0 || modDate2.CompareTo(modDate3) != 0 || modDate1.CompareTo(modDate3) != 0)
                            {
                                //get the newest date and use this path to copy to the other paths
                                //(needs to be greater than or EQUAL TO just incase two of the files have the same date)

                                //first date is the newest
                                if (modDate1 >= modDate2 && modDate1 >= modDate3)
                                {
                                    compareDate = modDate1;
                                    compareFullPath = fullPath1;
                                    comparePath = path1;
                                }

                                //second date is the newest
                                if (modDate2 >= modDate1 && modDate2 >= modDate3)
                                {
                                    compareDate = modDate2;
                                    compareFullPath = fullPath2;
                                    comparePath = path2;
                                }

                                //third date is the newest
                                if (modDate3 >= modDate1 && modDate3 >= modDate2)
                                {
                                    compareDate = modDate3;
                                    compareFullPath = fullPath3;
                                    comparePath = path3;
                                }
                                //MessageBox.Show(pathEntity.ComparePath + "not the same created time file" + modDate1.ToString() + "---" + modDate2.ToString() + "---" + modDate3.ToString() + "-----(" + compareDate.ToString() + ")" );

                                //go through all entities and copy the newest file to the other files with
                                //the same compare path

                                //could go back and change this so it didn't go through all the entities
                                //just check the 1st 3, then the 2nd, then the 3rd etc. )
                                foreach (PathEntity pe in allPathEntity)
                                {
                                    //can't replace a file with itself will throw file already in use error
                                    if (pe.ComparePath == comparePath && pe.FullPath != compareFullPath)
                                    {
                                        File.Copy(compareFullPath, pe.FullPath, true);
                                        log.Insert("(Updated) source:'" + compareFullPath + "' destination:'" + pe.FullPath + "'");
                                    }
                                }
                            }
                        }
                        //every third instance go back to zero so that it can start over again
                        if (ct == 3)
                            ct = 0;
                    }
                }//end foreach

            }
            catch (Exception ex) { LogError.Log("Program:WebFolderSync;Method:UpdateFiles()" + ex.Message); }
        }

        private void DeleteFiles()
        {
            try
            {
                //go through every path entity
                foreach (PathEntity pathEntity in allPathEntity)
                {
                    //if there is two copies of the file then it has been deleted
                    //from one of the folders and needs to delete the two remaining copies
                    if (pathEntity.Count == 2)
                    {
                        //when the path entity with two instance exists
                        //try to delete the other two instances 
                        //(check if they exists, might have already deleted it or it is the deleted file)
                        
                        if (pathEntity.FullPath.Contains(SERVMAIL_WEB_FOLDER))
                        {
                            if (File.Exists(SERVERDB_WEB_FOLDER + pathEntity.ComparePath))
                                File.Delete(SERVERDB_WEB_FOLDER + pathEntity.ComparePath);

                            if (File.Exists(SERVER_DB_WEB_FOLDER + pathEntity.ComparePath))
                                File.Delete(SERVER_DB_WEB_FOLDER + pathEntity.ComparePath);

                            log.Insert("(DELETED) source:'" + pathEntity.FullPath + "'");
                        }
                        else if (pathEntity.FullPath.Contains(SERVERDB_WEB_FOLDER))
                        {
                            if (File.Exists(SERVMAIL_WEB_FOLDER + pathEntity.ComparePath))
                                File.Delete(SERVMAIL_WEB_FOLDER + pathEntity.ComparePath);

                            if (File.Exists(SERVER_DB_WEB_FOLDER + pathEntity.ComparePath))
                                File.Delete(SERVER_DB_WEB_FOLDER + pathEntity.ComparePath);

                            log.Insert("(DELETED) source:'" + pathEntity.FullPath + "'");
                        }
                        else if (pathEntity.FullPath.Contains(SERVER_DB_WEB_FOLDER))
                        {
                            if (File.Exists(SERVMAIL_WEB_FOLDER + pathEntity.ComparePath))
                                File.Delete(SERVMAIL_WEB_FOLDER + pathEntity.ComparePath);

                            if (File.Exists(SERVERDB_WEB_FOLDER + pathEntity.ComparePath))
                                File.Delete(SERVERDB_WEB_FOLDER + pathEntity.ComparePath);

                            log.Insert("(DELETED) source:'" + pathEntity.FullPath + "'");
                        }
                    }
                }
            }
            catch (Exception ex) { LogError.Log("Program:WebFolderSync;Method:DeleteFiles()" + ex.Message); }
        }

        private void AddFiles()
        {
            try
            {
                //go through every path entity
                foreach (PathEntity pathEntity in allPathEntity)
                {
                    //if there is only one copy of the file then it has been added and
                    //needs to be added to the other two folders
                    if (pathEntity.Count == 1)
                    {
                        //when the path entity with one instance exists
                        //copy it to the two other locations
                        
                        if (pathEntity.FullPath.Contains(SERVMAIL_WEB_FOLDER))
                        {//SERVMAIL
                            foreach (PathEntity copyPathEntity in allPathEntity)
                            {
                                if (copyPathEntity.ComparePath == pathEntity.ComparePath)
                                {
                                    File.Copy(pathEntity.FullPath, SERVER_DB_WEB_FOLDER + pathEntity.ComparePath, true);
                                    File.Copy(pathEntity.FullPath, SERVERDB_WEB_FOLDER + pathEntity.ComparePath, true);
                                    log.Insert("(ADDED) source:'" + pathEntity.FullPath + "' destination: '" + SERVER_DB_WEB_FOLDER + pathEntity.ComparePath + "'");
                                    log.Insert("(ADDED) source:'" + pathEntity.FullPath + "' destination: '" + SERVERDB_WEB_FOLDER + pathEntity.ComparePath + "'");
                                }
                            }
                        }
                        else if (pathEntity.FullPath.Contains(SERVERDB_WEB_FOLDER))
                        {//SERVERDB
                            foreach (PathEntity copyPathEntity in allPathEntity)
                            {
                                if (copyPathEntity.ComparePath == pathEntity.ComparePath)
                                {
                                    File.Copy(pathEntity.FullPath, SERVER_DB_WEB_FOLDER + pathEntity.ComparePath, true);
                                    File.Copy(pathEntity.FullPath, SERVMAIL_WEB_FOLDER + pathEntity.ComparePath, true);
                                    log.Insert("(ADDED) source:'" + pathEntity.FullPath + "' destination: '" + SERVER_DB_WEB_FOLDER + pathEntity.ComparePath + "'");
                                    log.Insert("(ADDED) source:'" + pathEntity.FullPath + "' destination: '" + SERVMAIL_WEB_FOLDER + pathEntity.ComparePath + "'");
                                }
                            }
                        }//SERVER_DB
                        else if (pathEntity.FullPath.Contains(SERVER_DB_WEB_FOLDER))
                        {
                            foreach (PathEntity copyPathEntity in allPathEntity)
                            {
                                if (copyPathEntity.ComparePath == pathEntity.ComparePath)
                                {
                                    File.Copy(pathEntity.FullPath, SERVERDB_WEB_FOLDER + pathEntity.ComparePath, true);
                                    File.Copy(pathEntity.FullPath, SERVMAIL_WEB_FOLDER + pathEntity.ComparePath, true);
                                    log.Insert("(ADDED) source:'" + pathEntity.FullPath + "' destination: '" + SERVERDB_WEB_FOLDER + pathEntity.ComparePath + "'");
                                    log.Insert("(ADDED) source:'" + pathEntity.FullPath + "' destination: '" + SERVMAIL_WEB_FOLDER + pathEntity.ComparePath + "'");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { LogError.Log("Program:WebFolderSync;Method:AddFiles()" + ex.Message); }
        }

        private void LoadPathEntities()
        {
            //get every path from every location excluding the misc folder
            allPathEntity = Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, SERVERDB_WEB_FOLDER, SERVER_DB_WEB_FOLDER, "misc");
            List<PathEntity> countPathEntity = new List<PathEntity>();
            Stack<string> stack = new Stack<string>();

            try
            {

                //sort list by comparePath
                //so that each item can be compared to the next
                allPathEntity.Sort(delegate(PathEntity p1, PathEntity p2) { return p1.ComparePath.CompareTo(p2.ComparePath); });

                bool first = true;
                foreach (PathEntity pathEntity in allPathEntity)
                {
                    //push first
                    if (first)
                    {
                        stack.Push(pathEntity.ComparePath);
                        first = false;
                    }
                    else
                    {
                        //check top element in stack and compare to pathEntity path
                        if (stack.Peek() == pathEntity.ComparePath)
                        {
                            stack.Push(pathEntity.ComparePath);
                        }//once they aren't equal
                        else if (stack.Peek() != pathEntity.ComparePath)
                        {
                            //make a entiry with the compare path and the count of that compare path
                            countPathEntity.Add(new PathEntity("", stack.Peek(), DateTime.Now, stack.Count));//listBoxTest.Items.Add(stack.Peek() + stack.Count);

                            //pop all elements
                            while (stack.Count > 0)
                            {
                                stack.Pop();
                            }

                            //push compare path into stack
                            stack.Push(pathEntity.ComparePath);
                        }
                    }
                }//end foreach

                //add the last element that is still in the stack
                countPathEntity.Add(new PathEntity("", stack.Peek(), DateTime.Now, stack.Count));//listBoxTest.Items.Add(stack.Peek() + stack.Count);

                //when the compare paths are equal add the count to the entities
                foreach (PathEntity pathEntity in allPathEntity)
                {
                    foreach (PathEntity countEntity in countPathEntity)
                    {
                        if (pathEntity.ComparePath == countEntity.ComparePath)
                        {
                            pathEntity.Count = countEntity.Count;
                        }
                    }
                }

                //foreach (PathEntity pathEntity in allPathEntity)
                //{
                //    listBoxTest.Items.Add(pathEntity.ComparePath + "---" + pathEntity.Count);
                //}

            }
            catch (Exception ex) { LogError.Log("Program:WebFolderSync;Method:LoadPathEntities()" + ex.Message); }
        }
    }
}

/*



            //List<PathEntity> servmailPathEntity = Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, "misc");
            //List<PathEntity> serverdbPathEntity = Paths.GetPathEntities(SERVERDB_WEB_FOLDER, "misc");
            //List<PathEntity> server_dbPathEntity = Paths.GetPathEntities(SERVER_DB_WEB_FOLDER, "misc");
            //List<PathEntity> compare1 = new List<PathEntity>();
            //List<PathEntity> compare2 = new List<PathEntity>();

            ////foreach (PathEntity pathEntity in Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, SERVERDB_WEB_FOLDER, SERVER_DB_WEB_FOLDER, "misc"))
            ////{
            ////    listBox1.Items.Add(pathEntity.ComparePath + "-" + pathEntity.ModifiedDate);
            ////}

            ////servmail
            //foreach (PathEntity pathEntity in servmailPathEntity)
            //{
            //    listBox1.Items.Add(pathEntity.ComparePath);
            //}

            ////serverdb
            //foreach (PathEntity pathEntity in serverdbPathEntity)
            //{
            //    listBox2.Items.Add(pathEntity.ComparePath);
            //}

            ////server_db
            //foreach (PathEntity pathEntity in server_dbPathEntity)
            //{
            //    listBox3.Items.Add(pathEntity.ComparePath);
            //}
            
            ////servmail & serverdb
            //foreach (PathEntity pathEntity in servmailPathEntity)
            //{
            //    foreach (PathEntity pathEntity2 in serverdbPathEntity)
            //    {
            //        if (pathEntity.ComparePath == pathEntity2.ComparePath)
            //        {
            //            listBox4.Items.Add(pathEntity.ComparePath);
            //            compare1.Add(new PathEntity("", pathEntity.ComparePath, DateTime.Now));
            //        }
            //    }
            //}

            ////serverdb & server_db
            //foreach (PathEntity pathEntity in serverdbPathEntity)
            //{
            //    foreach (PathEntity pathEntity2 in server_dbPathEntity)
            //    {
            //        if (pathEntity.ComparePath == pathEntity2.ComparePath)
            //        {
            //            listBox5.Items.Add(pathEntity.ComparePath);
            //            compare2.Add(new PathEntity("", pathEntity.ComparePath, DateTime.Now));
            //        }
            //    }
            //}

            //foreach (PathEntity pathEntity in compare1)
            //{
            //    foreach (PathEntity pathEntity2 in compare2)
            //    {
            //        if (pathEntity.ComparePath == pathEntity2.ComparePath)
            //        {
            //            listBox6.Items.Add(pathEntity.ComparePath);

            //        }
            //    }
            //}





            //List<PathEntity> servmailPathEntity = Paths.GetPathEntities(@"c:\servers\servmail", "misc");
            //List<PathEntity> server_dbPathEntity = Paths.GetPathEntities(@"c:\servers\server_db");
            
            //string[] servmailFiles = Paths.GetFiles(@"c:\servers\servmail", "misc");
            //string[] server_dbFiles = Paths.GetFiles(@"c:\servers\server_db");
            ////find difference
            ////what's not in servermail
            //IEnumerable<string> difference = server_dbFiles.Except(servmailFiles);

            //foreach (string s in difference)
            //    listBox1.Items.Add(s);

            ////string[] servmailFiles = Paths.GetFiles(@"c:\servers\servmail", "misc");
            ////string[] server_dbFiles = Paths.GetFiles(@"c:\servers\server_db", "misc");

            //IEnumerable<string> intersect = server_dbFiles.Intersect(servmailFiles);

            //foreach (string s in intersect)
            //    listBox1.Items.Add(s);

            //foreach (string s in servmailFiles)
            //    listBox1.Items.Add(s);

            //foreach (string s in server_dbFiles)
            //    listBox1.Items.Add(s);


            //List<PathEntity> servmailPathEntity = Paths.GetPathEntities("servmail",SERVMAIL_WEB_FOLDER, "misc");
            //List<PathEntity> server_dbPathEntity = Paths.GetPathEntities("server_db",SERVER_DB_WEB_FOLDER, "misc");
            //List<PathEntity> serverdbPathEntity = Paths.GetPathEntities("serverdb",SERVERDB_WEB_FOLDER, "misc");



            //foreach (PathEntity servmailPathEntity in Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, "misc"))
            //{
            //    foreach (PathEntity server_dbPathEntity in Paths.GetPathEntities(SERVER_DB_WEB_FOLDER, "misc"))
            //    {
            //        if (servmailPathEntity.ComparePath == server_dbPathEntity.ComparePath)
            //        {
            //            //servmail and server_db that are the same
            //            compareList.Add(servmailPathEntity.ComparePath);
            //        }
            //        break;
            //    }
            //}

            //foreach (PathEntity serverdbPathEntity in Paths.GetPathEntities(SERVERDB_WEB_FOLDER, "misc"))
            //{
            //    foreach (string compare in compareList)
            //    {
            //        if (compare == serverdbPathEntity.ComparePath)
            //        {
            //            //serverdb that is the same as (servmail and server_db)
            //            compareList2.Add(compare);
            //        }
            //        break;
            //    }
            //}

            //foreach (string compare in compareList)
            //{
            //    foreach (string compare2 in compareList2)
            //    {
            //        if(compare == compare2)
            //        {
            //            compareList3.Add(compare);
            //        }
            //        break;
            //    }
            //}

            ////loop servmail
            //foreach (PathEntity servmailPathEntity in Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, "misc"))
            //{
            //    //loop server_db
            //    foreach (PathEntity server_dbPathEntity in Paths.GetPathEntities(ERVER_DB_WEB_FOLDER, "misc"))
            //    {
            //        //same file servmail & server_db
            //        if (servmailPathEntity.ComparePath == server_dbPathEntity.ComparePath)
            //        {
            //                //servmail less (older)
            //                if (servmailPathEntity.ModifiedDate < server_dbPathEntity.ModifiedDate)
            //                {
            //                    File.Copy(server_dbPathEntity.FullPath, servmailPathEntity.FullPath, true);
            //                }
            //                //server_db less (older)
            //                if (servmailPathEntity.ModifiedDate > server_dbPathEntity.ModifiedDate)
            //                {
            //                    File.Copy(servmailPathEntity.FullPath, server_dbPathEntity.FullPath, true);
            //                }
            //            //loop serverdb
            //            foreach (PathEntity serverdbPathEntity in Paths.GetPathEntities(SERVERDB_WEB_FOLDER, "misc"))
            //            {
            //                //same as servermail or serverdb
            //                if (serverdbPathEntity.ComparePath == server_dbPathEntity.ComparePath || serverdbPathEntity.ComparePath == servmailPathEntity.ComparePath)
            //                {


            //                }
            //            }
            //        }
                        
            //    }
            //}

            ////loop through every servmail entity
            //foreach (PathEntity servmailPathEntity in Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, "misc"))
            //{
            //    //in each loop loop through serber_db
            //    foreach (PathEntity server_dbPathEntity in Paths.GetPathEntities(SERVER_DB_WEB_FOLDER, "misc"))
            //    {
            //        //if file paths are equal
            //        if (servmailPathEntity.ComparePath == server_dbPathEntity.ComparePath)
            //        {
            //            //if servmail file is older than server_db file
            //            if (servmailPathEntity.ModifiedDate < server_dbPathEntity.ModifiedDate)
            //            {
            //                //add the item to the listbox
            //                //listBox1.Items.Add(servmailPathEntity.FullPath);

            //                //copy new record over old record
            //                File.Copy(server_dbPathEntity.FullPath, servmailPathEntity.FullPath, true);
            //            }
            //            //can break so that it doesn't keep checking after finding a match
            //            break;
            //        }
            //    }
            //}

            //deletes files from servmail that are not on server_db

            string[] servmailFiles = Paths.GetFiles(SERVMAIL_WEB_FOLDER, "misc");
            string[] server_dbFiles = Paths.GetFiles(SERVER_DB_WEB_FOLDER, "misc");

            //gets the files not in servmail
            //IEnumerable<string> difference = server_dbFiles.Except(servmailFiles);
            //gets the files servmail has but 
            IEnumerable<string> difference = servmailFiles.Except(server_dbFiles);

            foreach (string s in difference)
                listBox1.Items.Add(s);

            ////loop through every servmail entity
            //foreach (PathEntity servmailPathEntity in Paths.GetPathEntities(SERVMAIL_WEB_FOLDER, "misc"))
            //{
            //    //in each loop loop through serber_db
            //    foreach (PathEntity server_dbPathEntity in Paths.GetPathEntities(SERVER_DB_WEB_FOLDER, "misc"))
            //    {
            //        //if file paths are equal
            //        if (servmailPathEntity.ComparePath == server_dbPathEntity.ComparePath)
            //        {
            //            //if servmail file is older than server_db file
            //            if (servmailPathEntity.ModifiedDate < server_dbPathEntity.ModifiedDate)
            //            {
            //                //add the item to the listbox
            //                listBox1.Items.Add(servmailPathEntity.FullPath);

            //                //copy new record over old record
            //                //File.Copy(server_dbPathEntity.FullPath, servmailPathEntity.FullPath, true);
            //            }
            //            //can break so that it doesn't keep checking after finding a match
            //            break;
            //        }
            //    }
            //}

*/

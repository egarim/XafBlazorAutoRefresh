using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using XafBlazorAutoRefresh.Module.BusinessObjects;

namespace XafBlazorAutoRefresh.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class BackgroundThreadController : ViewController
    {
        SimpleAction DeleteAll;
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public BackgroundThreadController()
        {
            InitializeComponent();
            this.TargetObjectType = typeof(Test);
            this.TargetViewType = ViewType.ListView;

            DeleteAll = new SimpleAction(this, "Delete all", "View");
            DeleteAll.Execute += DeleteAll_Execute;
            

            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        private void DeleteAll_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            this.View.ObjectSpace.Delete(this.View.ObjectSpace.CreateCollection(this.TargetObjectType));
            this.View.ObjectSpace.CommitChanges();
            // Execute your business logic (https://docs.devexpress.com/eXpressAppFramework/112737/).
        }
      
        Random random = new Random();
        BackgroundWorker backgroundWorker;
        protected override void OnActivated()
        {
            base.OnActivated();
            //SELECT GETDATE();
            this.View.QueryCanClose += View_QueryCanClose;
          
            EmulateLongProcess();
            RefreshTimerWithBackgroundThread();
            // Perform various tasks depending on the target View.
        }

        private void View_QueryCanClose(object sender, CancelEventArgs e)
        {
            TryToCancel();
        }

        private void TryToCancel()
        {
            if (backgroundWorker != null)
            {
                backgroundWorker.CancelAsync();
            }
        }

        void RefreshTimerWithBackgroundThread()
        {
          

         

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (backgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }


                    System.Threading.Thread.Sleep(100);
                }

            }
            void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                if (!e.Cancelled)
                {
                    EmulateLongProcess();
                }

            }
            backgroundWorker.RunWorkerAsync();
         
        }

        private void EmulateLongProcess()
        {
            var Date = DateTime.Now.ToString("G");

            //var Date= XpObjectSpace.Session.ExecuteScalar("SELECT GETDATE()").ToString();
            var CurrentObject = this.View.ObjectSpace.CreateObject<Test>();
            CurrentObject.LastRefresh = Date;
            System.Threading.Thread.Sleep(random.Next(5000));
            this.View.ObjectSpace.CommitChanges();
            this.View.Refresh();
            RefreshTimerWithBackgroundThread();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
            this.View.QueryCanClose -= View_QueryCanClose;
            TryToCancel();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;

namespace PatcherWPF.Source
{
    class FormConnector
    {
        private Window mMainForm;

        private List<Window> mConnectedForms = new List<Window>();

        private Point mMainLocation;

        public FormConnector(Window mainForm)
        {
            this.mMainForm = mainForm;
            this.mMainLocation = new Point(this.mMainForm.Left, this.mMainForm.Top);
            this.mMainForm.LocationChanged += new EventHandler(MainForm_LocationChanged);
        }

        public void ConnectForm(Window form)
        {
            if (!this.mConnectedForms.Contains(form))
            {
                this.mConnectedForms.Add(form);
            }
        }

        void MainForm_LocationChanged(object sender, EventArgs e)
        {
            Point relativeChange = new Point(this.mMainForm.Left - this.mMainLocation.X, this.mMainForm.Left - this.mMainLocation.Y);
            foreach (Window form in this.mConnectedForms)
            {
                form.Left = form.Left + relativeChange.X;
                form.Top = form.Top + relativeChange.Y;
            }

            this.mMainLocation = new Point(this.mMainForm.Left, this.mMainForm.Left);
        }
    }
}

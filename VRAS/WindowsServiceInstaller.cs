using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        // C:\data\wingds\windows\VRAS\VRAS\bin\Debug>C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /ServiceName=zzzz /u VRAS.exe
        private string defaultServiceName;
        private ServiceInstaller serviceInstaller;

        /// <summary>
        /// Public Constructor for WindowsServiceInstaller.
        /// - Put all of your Initialization code here.
        /// </summary>
        public WindowsServiceInstaller()
        {
            this.defaultServiceName = "VRAS";
            this.BeforeInstall += new InstallEventHandler(this.ProjectInstaller_BeforeInstall);
            this.BeforeUninstall += new InstallEventHandler(this.ProjectInstaller_BeforeUninstall);

            ServiceProcessInstaller serviceProcessInstaller =
                               new ServiceProcessInstaller();
            this.serviceInstaller = new ServiceInstaller();

            // # Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            // # Service Information
            this.serviceInstaller.StartType = ServiceStartMode.Automatic;

            // # This must be identical to the WindowsService.ServiceBase name
            // # set in the constructor of WindowsService.cs
            // serviceInstaller.ServiceName = "My Windows Service";
            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(this.serviceInstaller);
        }

        private void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            // Configure ServiceName 
            if (!String.IsNullOrEmpty(this.Context.Parameters["ServiceName"]))
            {
                this.serviceInstaller.ServiceName = this.Context.Parameters["ServiceName"];
                this.serviceInstaller.DisplayName = this.Context.Parameters["ServiceName"];
            }
            else
            {
                this.serviceInstaller.ServiceName = this.defaultServiceName;
                this.serviceInstaller.DisplayName = this.defaultServiceName;
            }
        }

        private void ProjectInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.Context.Parameters["ServiceName"]))
            {
                this.serviceInstaller.ServiceName = this.Context.Parameters["ServiceName"];
            }
            else
            {
                this.serviceInstaller.ServiceName = this.defaultServiceName;
            }
        }
    }
}

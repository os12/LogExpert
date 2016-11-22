﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
  class LogExpertApplicationContext : ApplicationContext
  {
    private LogExpertProxy proxy;

    public LogExpertApplicationContext(LogExpertProxy proxy, LogTabWindow firstLogWin)
    {
      this.proxy = proxy;
      this.proxy.LastWindowClosed += new LogExpertProxy.LastWindowClosedEventHandler(proxy_LastWindowClosed);
      firstLogWin.Show();
    }

    void proxy_LastWindowClosed(object sender, EventArgs e)
    {
      ExitThread();
      Application.Exit();
    }
  }
}

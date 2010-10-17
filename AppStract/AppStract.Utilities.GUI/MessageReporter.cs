﻿#region Copyright (C) 2009-2010 Simon Allaeys

/*
    Copyright (C) 2009-2010 Simon Allaeys
 
    This file is part of AppStract

    AppStract is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AppStract is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AppStract.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using AppStract.Core.System.Logging;

namespace AppStract.Utilities.GUI
{
  public partial class MessageReporter : Form
  {

    #region Types

    /// <summary>
    /// Callback method for the output of <see cref="MessageFormatter"/>.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageDetails"></param>
    private delegate void ReportMessageEventHandler(string message, string messageDetails);

    /// <summary>
    /// Formats messages to be displayed by <see cref="MessageReporter"/>.
    /// The output is formatted the same as in the log files.
    /// </summary>
    private class MessageFormatter : NullLogger
    {

      private readonly ReportMessageEventHandler _reportMessage;

      public MessageFormatter(ReportMessageEventHandler reportMessageEventHandler)
      {
        LogLevel = LogLevel.All;
        _reportMessage = reportMessageEventHandler;
      }

      protected override void Write(LogMessage message)
      {
        _reportMessage(message.Message, FormatException(message.Exception, LogLevel));
      }

    }

    #endregion

    #region Variables

    private const int _DefaultFullHeightAddition = 200;
    private readonly int _defaultCollapsedHeight;
    private readonly MessageFormatter _formatter;

    #endregion

    #region Constructors

    private MessageReporter()
    {
      InitializeComponent();
      _formatter = new MessageFormatter(ReportMessage);
      _defaultCollapsedHeight = Size.Height;
    }

    #endregion

    #region Public Static Methods

    public static DialogResult Show(string message, string title)
    {
      return Show(message, title, string.Empty);
    }

    public static DialogResult Show(string message, string title, string messageDetails)
    {
      return Show(message, title, messageDetails, null);
    }

    public static DialogResult Show(string message, string title, string messageDetails, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
    {
      return Show(message, title, messageDetails, null, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    public static DialogResult Show(string message, string title, Exception exception)
    {
      return Show(message, title, string.Empty, exception);
    }

    public static DialogResult Show(string message, string title, string messageDetails, Exception exception)
    {
      return Show(message, title, messageDetails, exception, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    public static DialogResult Show(string message, string title, string messageDetails, Exception exception, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
    {
      var reporter = new MessageReporter();
      var logLevel = GetBoxIconAsLogLevel(messageBoxIcon);
      reporter.ApplyButtonConfiguration(messageBoxButtons);
      reporter.ApplyIconConfiguration(messageBoxIcon);
      reporter.Text = title;
      reporter._txtMessageDetail.Text = messageDetails ?? "";
      reporter._formatter.Log(new LogMessage(logLevel, message, exception));
      reporter.ShowDialog();
      return reporter.DialogResult;
    }

    #endregion

    #region Private Methods

    private void ApplyButtonConfiguration(MessageBoxButtons buttons)
    {
      switch (buttons)
      {
        case MessageBoxButtons.AbortRetryIgnore:
          _button1.Text = "Abort";
          _button1.Tag = DialogResult.Abort;
          _button1.Visible = true;
          _button2.Text = "Retry";
          _button2.Tag = DialogResult.Retry;
          _button2.Visible = true;
          _button3.Text = "Ignore";
          _button3.Tag = DialogResult.Ignore;
          _button3.Visible = true;
          break;
        case MessageBoxButtons.OK:
          _button1.Visible = false;
          _button2.Visible = false;
          _button3.Text = "OK";
          _button3.Tag = DialogResult.OK;
          _button3.Visible = true;
          break;
        case MessageBoxButtons.OKCancel:
          _button1.Visible = false;
          _button2.Text = "OK";
          _button2.Tag = DialogResult.OK;
          _button2.Visible = true;
          _button3.Text = "Cancel";
          _button3.Tag = DialogResult.Cancel;
          _button3.Visible = true;
          break;
        case MessageBoxButtons.RetryCancel:
          _button1.Visible = false;
          _button2.Text = "Retry";
          _button2.Tag = DialogResult.Retry;
          _button2.Visible = true;
          _button3.Text = "Cancel";
          _button3.Tag = DialogResult.Cancel;
          _button3.Visible = true;
          break;
        case MessageBoxButtons.YesNo:
          _button1.Visible = false;
          _button2.Text = "Yes";
          _button2.Tag = DialogResult.Yes;
          _button2.Visible = true;
          _button3.Text = "No";
          _button3.Tag = DialogResult.No;
          _button3.Visible = true;
          break;
        case MessageBoxButtons.YesNoCancel:
          _button1.Text = "Yes";
          _button1.Tag = DialogResult.Yes;
          _button1.Visible = true;
          _button2.Text = "No";
          _button2.Tag = DialogResult.No;
          _button2.Visible = true;
          _button3.Text = "Cancel";
          _button3.Tag = DialogResult.Cancel;
          _button3.Visible = true;
          break;
      }
    }

    private void ApplyIconConfiguration(MessageBoxIcon icon)
    {
      // Available for future use.
    }

    private void ReportMessage(string message, string formattedException)
    {
      _btnExpand.Enabled = !string.IsNullOrEmpty(_txtMessageDetail.Text) || !string.IsNullOrEmpty(formattedException);
      _lblMessage.Text = message ?? "";
      if (formattedException != null)
      {
        if (!string.IsNullOrEmpty(_txtMessageDetail.Text))
          _txtMessageDetail.Text += "\r\r";
        _txtMessageDetail.Text += formattedException;
      }
    }

    #endregion

    #region Form EventHandlers

    private void ButtonExpand_Click(object sender, EventArgs e)
    {
      var collapsed = MaximumSize.Height != 0;
      if (collapsed)
      {
        _btnExpand.Image = Properties.Resources.Up;
        _layoutContent.RowStyles[2].SizeType = SizeType.Percent;
        _layoutContent.RowStyles[2].Height = 100.0f;
        MaximumSize = new Size();
        Size = new Size(Size.Width, _defaultCollapsedHeight + _DefaultFullHeightAddition);
        MinimumSize = new Size(MinimumSize.Width, MinimumSize.Height + _DefaultFullHeightAddition/2);
      }
      else
      {
        _btnExpand.Image = Properties.Resources.Down;
        _layoutContent.RowStyles[2].SizeType = SizeType.Absolute;
        _layoutContent.RowStyles[2].Height = 1.0f;
        Size = new Size(Size.Width, 150);
        MinimumSize = new Size(MinimumSize.Width, MinimumSize.Height - _DefaultFullHeightAddition/2);
        MaximumSize = Size;
      }
    }

    private void ButtonClose_Click(object sender, EventArgs e)
    {
      if (sender is Button)
        DialogResult = (DialogResult)(sender as Control).Tag;
      Close();
    }

    #endregion

    #region Private Static Methods

    private static LogLevel GetBoxIconAsLogLevel(MessageBoxIcon boxIcon)
    {
      if (boxIcon == MessageBoxIcon.Exclamation)
        return LogLevel.Warning;
      if (boxIcon == MessageBoxIcon.Error)
        return LogLevel.Error;
      if (boxIcon == MessageBoxIcon.Stop)
        return LogLevel.Critical;
      return LogLevel.Information;
    }

    #endregion

  }
}
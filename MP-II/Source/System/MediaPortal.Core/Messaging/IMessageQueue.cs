﻿#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;

namespace MediaPortal.Core.Messaging
{
  public delegate void MessageReceivedHandler(QueueMessage message);

  public interface IMessageQueue
  {
    event MessageReceivedHandler MessageReceived;

    /// <summary>
    /// Gets the information if this queue has subscribers.
    /// </summary>
    bool HasSubscribers { get;}

    /// <summary>
    /// Gets the message filters.
    /// </summary>
    IList<IMessageFilter> Filters { get;}

    /// <summary>
    /// Sends the specified <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void Send(QueueMessage message);
  }
}

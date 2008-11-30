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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MediaPortal.Core.MediaManagement.MLQueries;

namespace MediaPortal.Media.ClientMediaManager.Views
{
  /// <summary>
  /// Holds the metadata of a view which is based on a local provider path.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Note: This class is serialized/deserialized by the <see cref="XmlSerializer"/>.
  /// If changed, this has to be taken into consideration.
  /// </para>
  /// </remarks>
  public class MediaLibraryViewMetadata : ViewMetadata
  {
    #region Protected fields

    protected IQuery _query;

    #endregion

    internal MediaLibraryViewMetadata(Guid viewId, string displayName, IQuery query,
        Guid? parentViewId, IEnumerable<Guid> mediaItemAspectIds) :
      base(viewId, displayName, parentViewId, mediaItemAspectIds)
    {
      _query = query;
    }

    /// <summary>
    /// Returns the media library query this view is based on.
    /// </summary>
    [XmlIgnore]
    public IQuery Query
    {
      get { return _query; }
    }

    #region Additional members for the XML serialization

    internal MediaLibraryViewMetadata() { }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlElement("QueryString")]
    public string XML_QueryString
    {
      get { return "Not used yet"; }
      set
      {
        // TODO
      }
    }

    #endregion
  }
}

#region Copyright (C) 2007-2013 Team MediaPortal

/*
    Copyright (C) 2007-2013 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Linq;
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.SystemResolver;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UiComponents.Media.Models;
using MediasitePlugin.ResourceProvider;
using www.sonicfoundry.com.Mediasite.Services60.Messages;

using System.Collections.Generic;


namespace MediasitePlugin
{
  /// <summary>
  /// Todo: Add comments.
  /// </summary>
  public class MediasitePlugin : IWorkflowModel
  {
    #region Consts
    private const string API_ENDPOINT = "http://ais.sofodev.com/mediasite/services60/edassixonefive.svc";
    private const string PRIVATE_KEY = "uTBisLe83ZgZExYUYcKkVA==";
    private const string PUBLIC_KEY = "EAJos+ME82eh+rCUA+96tA==";
    private const string APPLICATION = "MediaPortal2";
    private const string SOFOSITE = "ais.sofodev.com";
    public const string MODEL_ID_STR = "89A89847-7523-47CB-9276-4EC544B8F19A";
    public static Guid MODEL_ID = new Guid(MODEL_ID_STR);
    private MediasiteHelper _msHelper;
    /// <summary>
    /// Another localized string resource.
    /// </summary>
    protected const string COMMAND_TRIGGERED_RESOURCE = "[Mediasite.ButtonTextCommandExecuted]";
    #endregion

    #region Protected properties

    /// <summary>
    /// This property holds a string that we will modify in this tutorial.
    /// </summary>
   
    private readonly ItemsList _presentations = new ItemsList();
    private readonly ItemsList _slides = new ItemsList();
    

    #endregion

    #region Ctor & maintainance

    /// <summary>
    /// Constructor... this one is called by the WorkflowManager when this model is loaded due to a screen reference.
    /// </summary>
    public MediasitePlugin()
    {

    }

    #endregion

    #region Public members

    /// <summary>
    /// Gets an ItemList of all Presentations.
    /// </summary>
    /// 
    public ItemsList Presentations
    {
      get
      {
        return _presentations;
      }
    }
    /// <summary>
    /// Gets an ItemList of slides for current selected Presentation.
    /// </summary>
    public ItemsList Slides
    {
      get
      {
        return _slides;
      }
    }

    /// <summary>
    /// Refreshes the contents of <see cref="Presentations"/> list.
    /// </summary>
    public void RefreshPresentations()
    {
      _presentations.Clear();
      foreach (PresentationDetails presentation in _msHelper.Presentations)
      {
        ListItem item = new ListItem("Name", presentation.Name);
        PresentationDetails localPresentation = presentation; // Keep local variable to avoid changing values in iterations
        item.Command = new MethodDelegateCommand(() => PlayVideo(localPresentation));
        _presentations.Add(item);
      }
      _presentations.FireChange();
    }

    private void LoadSlides(PresentationDetails presentation)
    {
      _slides.Clear();
      foreach (SlideDetails slide in _msHelper.LoadSlides(presentation))
      {
        string url = presentation.FileServerUrl.ToLower().Replace(_msHelper.SiteName, "Public") + @"/" +
          presentation.Id + @"/" + String.Format(_msHelper.GetSlideContent(presentation.Content).FileNameWithExtension, slide.Number.ToString("D" + 4));

        string title = slide.Title;
        if (string.IsNullOrWhiteSpace(title))
          title = string.Format("Time: " + TimeSpan.FromMilliseconds(slide.Time));

        ListItem item = new ListItem("Name", title);
        item.SetLabel("URL", url);
        SlideDetails localSlide = slide;
        item.Command = new MethodDelegateCommand(() => ShowSlide(localSlide));
        _slides.Add(item);
      }
      _slides.FireChange();
    }

    private void ShowSlide(SlideDetails slide)
    {
      IPlayerContextManager pcm = ServiceRegistration.Get<IPlayerContextManager>();
      IPlayerContext ctx = pcm.GetPlayerContext(PlayerChoice.CurrentPlayer);
      if (ctx == null)
        return;
      MediaSitePlayer mediaSitePlayer = ctx.CurrentPlayer as MediaSitePlayer;
      if (mediaSitePlayer ==  null)
        return;

      mediaSitePlayer.CurrentTime = TimeSpan.FromMilliseconds(slide.Time);
    }

    private void PlayVideo(PresentationDetails presentation)
    {
      LoadSlides(presentation);

      IDictionary<Guid, MediaItemAspect> aspects = new Dictionary<Guid, MediaItemAspect>();

      MediaItemAspect providerResourceAspect;
      aspects[ProviderResourceAspect.ASPECT_ID] = providerResourceAspect = new MediaItemAspect(ProviderResourceAspect.Metadata);
      MediaItemAspect mediaAspect;
      aspects[MediaAspect.ASPECT_ID] = mediaAspect = new MediaItemAspect(MediaAspect.Metadata);
      MediaItemAspect videoAspect;
      aspects[VideoAspect.ASPECT_ID] = videoAspect = new MediaItemAspect(VideoAspect.Metadata);

      providerResourceAspect.SetAttribute(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH, RawUrlMediaProvider.ToProviderResourcePath(presentation.VideoUrl).Serialize());
      providerResourceAspect.SetAttribute(ProviderResourceAspect.ATTR_SYSTEM_ID, ServiceRegistration.Get<ISystemResolver>().LocalSystemId);

      mediaAspect.SetAttribute(MediaAspect.ATTR_MIME_TYPE, MediaSitePlayer.MEDIASITE_MIMETYPE);
      mediaAspect.SetAttribute(MediaAspect.ATTR_TITLE, presentation.Name);
      videoAspect.SetAttribute(VideoAspect.ATTR_STORYPLOT, presentation.Description);

      MediaItem mediaItem = new MediaItem(Guid.Empty, aspects);

      PlayItemsModel.PlayItem(mediaItem);
    }

    #endregion

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return MODEL_ID; }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      _msHelper = new MediasiteHelper(API_ENDPOINT,PRIVATE_KEY,PUBLIC_KEY,APPLICATION,SOFOSITE);
      RefreshPresentations();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
      // We could initialize some data here when changing the media navigation state
    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
    }

    public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
    {
    }

    public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
    {
    }

    public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
    {
      return ScreenUpdateMode.AutoWorkflowManager;
    }

    #endregion
  }
}

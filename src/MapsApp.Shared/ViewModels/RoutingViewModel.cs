﻿// /*******************************************************************************
//  * Copyright 2017 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

using Esri.ArcGISRuntime.ExampleApps.MapsApp.Commands;
using Esri.ArcGISRuntime.ExampleApps.MapsApp.Helpers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Esri.ArcGISRuntime.ExampleApps.MapsApp.ViewModels
{
    class RoutingViewModel : BaseViewModel
    {
        private GeocodeResult _fromPlace;
        private GeocodeResult _toPlace;
        private RouteResult _route;
        private Viewpoint _areaOfInterest;
        private ICommand _clearRouteCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingViewModel"/> class.
        /// </summary>
        public RoutingViewModel()
        {
            
        }

        /// <summary>
        /// Gets or sets the end location for the route
        /// </summary>
        public GeocodeResult FromPlace
        {
            get { return _fromPlace; }
            set
            {
                if (_fromPlace != value)
                {
                    _fromPlace = value;
                    GetRouteAsync();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the end location for the route
        /// </summary>
        public GeocodeResult ToPlace
        {
            get { return _toPlace; }
            set
            {
                if (_toPlace != value)
                {
                    _toPlace = value;
                    GetRouteAsync();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the route 
        /// </summary>
        public RouteResult Route
        {
            get { return _route; }
            set
            {
                if (_route != value)
                {
                    _route = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current area of interest
        /// </summary>
        public Viewpoint AreaOfInterest
        {
            get
            {
                return _areaOfInterest;
            }

            set
            {
                if (_areaOfInterest != value)
                {
                    _areaOfInterest = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the command to cancel the location search and clear the pin off the map
        /// </summary>
        public ICommand ClearRouteCommand
        {
            get
            {
                return _clearRouteCommand ?? (_clearRouteCommand = new DelegateCommand(
                    (x) =>
                    {
                        Route = null;
                    }));
            }
        }

        /// <summary>
        /// Gets the router for the map
        /// </summary>
        internal RouteTask Router { get; set; }


        private async Task GetRouteAsync()
        {
            if (Router == null)
            {
                try
                {
                    Router = await RouteTask.CreateAsync(new Uri(Configuration.RouteUrl));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return;
                }
            }
            // set the route parameters
            var routeParams = await Router.CreateDefaultParametersAsync();
            routeParams.ReturnDirections = true;
            routeParams.ReturnRoutes = true;            

            // add route stops as parameters
            if (FromPlace != null && ToPlace != null)
            {
                try
                {
                    routeParams.SetStops(new List<Stop>() { new Stop(FromPlace.RouteLocation),
                                                            new Stop(ToPlace.RouteLocation) });

                    Route = await Router.SolveRouteAsync(routeParams);

                    //Get extent of the route and buffer it
                    var routeExtent = Route.Routes.FirstOrDefault().RouteGeometry.Extent;
                    var bufferedExtent = GeometryEngine.Buffer(routeExtent, routeExtent.Height * 0.2);

                    // Set viewpoint to the route's extent
                    AreaOfInterest = new Viewpoint(bufferedExtent);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

        }
    }
}

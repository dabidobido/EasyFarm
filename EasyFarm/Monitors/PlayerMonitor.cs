﻿
/*///////////////////////////////////////////////////////////////////
<EasyFarm, general farming utility for FFXI.>
Copyright (C) <2013>  <Zerolimits>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
*/
///////////////////////////////////////////////////////////////////

using EasyFarm.Classes;
using FFACETools;
using System.Timers;
using System.Linq;

namespace EasyFarm.FarmingTool
{
    public class PlayerMonitor : BaseMonitor
    {
        private UnitService _units;
        private bool _detected = false;

        public PlayerMonitor(FFACE fface)
            : base(fface)
        {
            this._units = new UnitService(fface);
        }

        protected override void CheckStatus(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                bool detected = _units.PCArray
                    .Any(x => UnitFilters.PCFilter(_fface, x));

                if (_detected != detected)
                {
                    OnChanged(new MonitorArgs<bool>(detected));
                    _detected = detected;
                }
            }
        }

        public bool Detected
        {
            get { return this._detected; }
        }
    }
}
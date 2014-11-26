﻿using SharpDX;

/*
    Copyright (C) 2014 Nikita Bernthaler
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace LightningRyze
{
    internal class Hero
    {
        public string Name { get; set; }

        public bool Visible { get; set; }

        public bool Dead { get; set; }

        public Vector3 LastPosition { get; set; }
    }
}
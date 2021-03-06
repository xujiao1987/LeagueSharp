﻿#region LICENSE

// Copyright 2014 - 2014 Support
// Kayle.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

#endregion

namespace Support.Disabled
{
    public class Kayle : PluginBase
    {
        public Kayle()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 525);
            R = new Spell(SpellSlot.R, 900);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                }

                if (W.CastCheck(Target, "ComboW"))
                {
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "HarassQ"))
                {
                }

                if (W.CastCheck(Target, "HarassW"))
                {
                }

                if (E.CastCheck(Target, "HarassE"))
                {
                }
            }
        }

        public override void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }

        public override void OnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.CastCheck(gapcloser.Sender, "GapcloserQ"))
            {
            }

            if (W.CastCheck(gapcloser.Sender, "GapcloserW"))
            {
            }

            if (E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
            }

            if (R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
                return;

            if (Q.CastCheck(unit, "InterruptQ"))
            {
            }

            if (W.CastCheck(unit, "InterruptW"))
            {
            }

            if (E.CastCheck(unit, "InterruptE"))
            {
            }

            if (R.CastCheck(unit, "InterruptR"))
            {
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboE", "使用 E", true);
            config.AddBool("ComboR", "使用 R", true);
            config.AddSlider("ComboCountR", "目标范围内的结果", 2, 0, 5);
            config.AddSlider("ComboHealthR", "血量低于%使用大招", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassW", "使用 W", true);
            config.AddBool("HarassE", "使用 E", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "使用 Q 打断突进", true);
            config.AddBool("GapcloserW", "使用 W 打断突进", true);
            config.AddBool("GapcloserE", "使用 E 打断突进", true);
            config.AddBool("GapcloserR", "使用 R 打断突进", true);

            config.AddBool("InterruptQ", "使用 Q 打断技能", true);
            config.AddBool("InterruptW", "使用 W 打断技能", true);
            config.AddBool("InterruptE", "使用 E 打断技能", true);
            config.AddBool("InterruptR", "使用 R 打断技能", true);
        }
    }
}
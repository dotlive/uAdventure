﻿using System;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace uAdventure.Core
{
    public abstract class AbstractEffect : IEffect, ICloneable
    {

        private Conditions conditions;

        public AbstractEffect()
        {

            conditions = new Conditions();
        }

        /**
         * @return the conditions
         */
        public Conditions getConditions()
        {

            return conditions;
        }

        /**
         * @param conditions
         *            the conditions to set
         */
        public void setConditions(Conditions conditions)
        {

            this.conditions = conditions;
        }

        /**
         * Returns the type of the effect.
         * 
         * @return Type of the effect
         */
        public abstract EffectType getType();

        public virtual object Clone()
        {
            AbstractEffect absEf = (AbstractEffect)this.MemberwiseClone();
            absEf.conditions = (Conditions)conditions.Clone();
            return absEf;
        }
        /*
    @Override
    public Object clone() throws CloneNotSupportedException
    {

       AbstractEffect absEf = (AbstractEffect) super.clone( );
       absEf.conditions = (Conditions) conditions.clone( );
      return absEf;
    }*/

        protected virtual string ToEffectString()
        {
            var name = getType().ToString().Replace("_", " ").ToLower();
            if(getType() == EffectType.CUSTOM_EFFECT)
            {
                name = Regex.Replace(GetType().Name.RemoveFromEnd("Effect"), "(\\B[A-Z])", " $1").ToLower();
            }

            var hasTargetId = this as HasTargetId;
            if (hasTargetId != null)
            {
                var targetId = hasTargetId.getTargetId();
                return name + " '" + targetId + "'";
            }
            return name;
        }

        public override string ToString()
        {
            var conditions = getConditions().GetConditionsList().Count > 0 ? getConditions().ToString() + "\n" : "";
            return conditions + ToEffectString();
        }
    }
}
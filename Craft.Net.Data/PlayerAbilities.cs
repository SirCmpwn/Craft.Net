using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Craft.Net.Data.Entities;

namespace Craft.Net.Data
{
    public class PlayerAbilities : INotifyPropertyChanged
    {
        public PlayerAbilities(PlayerEntity entity)
        {
            FirePropertyChanged = false;
            IsFlying = false;
            MayFly = false;
            Invulnerable = false;
            InstantMine = false;
            FirePropertyChanged = true;
            WalkingSpeed = 12;
            FlyingSpeed = 25;
            PlayerEntity = entity;
        }

        private bool isFlying;
        private bool mayFly;
        private bool invulnerable;
        private bool instantMine;
        private byte flyingSpeed;
        private byte walkingSpeed;

        public bool IsFlying
        {
            get { return isFlying; }
            set
            {
                isFlying = value;
                OnPropertyChanged("IsFlying");
            }
        }

        public bool MayFly
        {
            get { return mayFly; }
            set
            {
                mayFly = value;
                OnPropertyChanged("MayFly");
            }
        }

        public bool Invulnerable
        {
            get { return invulnerable; }
            set
            {
                invulnerable = value;
                OnPropertyChanged("Invulnerable");
            }
        }

        public bool InstantMine
        {
            get { return instantMine; }
            set
            {
                instantMine = value;
                OnPropertyChanged("InstantMine");
            }
        }

        public byte FlyingSpeed
        {
            get { return flyingSpeed; }
            set
            {
                flyingSpeed = value;
                OnPropertyChanged("FlyingSpeed");
            }
        }

        public byte WalkingSpeed
        {
            get { return walkingSpeed; }
            set
            {
                walkingSpeed = value;
                OnPropertyChanged("WalkingSpeed");
            }
        }

        private PlayerEntity PlayerEntity { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal bool FirePropertyChanged { get; set; }

        public void OnPropertyChanged(string propertyName)
        {
            if (!FirePropertyChanged)
                return;
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(PlayerEntity, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IsFlying: " + IsFlying);
            sb.Append(", MayFly: " + MayFly);
            sb.Append(", Invulnerable: " + Invulnerable);
            sb.Append(", InstantMine: " + InstantMine);
            sb.Append(", WalkingSpeed: " + WalkingSpeed);
            sb.Append(", FlyingSpeed: " + FlyingSpeed);
            return sb.ToString();
        }
    }
}

using System;
using System.ComponentModel;
using System.Threading;
using Craft.Net.Data.Events;
using Craft.Net.Data.Windows;
using Craft.Net.Metadata;
namespace Craft.Net.Data.Entities
{
    public class PlayerEntity : LivingEntity
    {
        public PlayerEntity(Difficulty difficulty)
        {
            Inventory = new InventoryWindow();
            for (int i = 0; i < Inventory.Length; i++)
                Inventory[i] = Slot.EmptySlot;
            SelectedSlot = InventoryWindow.HotbarIndex;
            bedUseTimer = new Timer(state =>
            {
                if (BedTimerExpired != null)
                    BedTimerExpired(this, null);
            });
            BedPosition = -Vector3.One;
            Health = 20;
            Food = 20;
            Abilities = new PlayerAbilities(this);
            ItemInMouse = Slot.EmptySlot;
            Difficulty = difficulty;
            TerrainCollision += OnTerrainCollision;
            FoodTickTimer = new Timer(discarded =>
                {
                    if (Food > 17 && Health < 20 && Health != 0) // TODO: HealthMax constant?
                        Health++;
                    if (Food == 0 && GameMode != GameMode.Creative)
                    {
                        switch (Difficulty)
                        {
                            case Difficulty.Hard:
                                if (Health > 0)
                                    Health--;
                                break;
                            case Difficulty.Normal:
                                if (Health > 1)
                                    Health--;
                                break;
                            default:
                                if (Health > 10)
                                    Health--;
                                break;
                        }
                    }
                }, null, 80 * Level.TickLength, 80 * Level.TickLength);
        }

        private double LastCollisionY = -1;
        private void OnTerrainCollision(object sender, EntityTerrainCollisionEventArgs entityTerrainCollisionEventArgs)
        {
            if (entityTerrainCollisionEventArgs.Direction.Y < -0.25f && !Abilities.IsFlying)
            {
                if (LastCollisionY != -1 && LastCollisionY > Position.Y)
                {
                    short diff = (short)((LastCollisionY - Position.Y) - 3);
                    if (diff > 0)
                        Health -= diff;
                }
                LastCollisionY = Position.Y;
            }
        }

        #region Properties

        #region Constants

        public override Size Size
        {
            get { return new Size(0.6, 1.62, 0.6); }
        }

        public override short MaxHealth
        {
            get { return 20; }
        }

        public static double Width
        {
            get { return 0.6; }
        }

        public static double Height
        {
            get { return 1.8; }
        }

        public static double Depth
        {
            get { return 0.6; }
        }

        public override float AccelerationDueToGravity
        {
            get { return 0.08f; }
        }

        public override float Drag
        {
            get { return 0.98f; }
        }

        #endregion

        #region Food & XP

        public short Food
        {
            get { return food; }
            set
            {
                if (GameMode == GameMode.Creative)
                    food = 20;
                else
                    food = value;
                OnPropertyChanged("Food");
            }
        }

        public float FoodSaturation
        {
            get { return foodSaturation; }
            set
            {
                foodSaturation = value;
                if (value == 0)
                    OnPropertyChanged("FoodSaturation");
            }
        }

        public float FoodExhaustion
        {
            get { return foodExhaustion; }
            set
            {
                if (Difficulty == Difficulty.Peaceful)
                    return;
                foodExhaustion = value;
                if (foodExhaustion > 4)
                {
                    if (FoodSaturation > 0)
                        FoodSaturation--;
                    else
                    {
                        if (Food > 0)
                            Food--;
                    }
                    foodExhaustion -= 4;
                }
                OnPropertyChanged("FoodExhaustion");
            }
        }

        protected Timer FoodTickTimer { get; set; }

        public int XpLevel
        {
            get { return xpLevel; }
            set
            {
                xpLevel = value;
                OnPropertyChanged("XpLevel");
            }
        }

        public int XpTotal
        {
            get { return xpTotal; }
            set
            {
                xpTotal = value;
                OnPropertyChanged("XpTotal");
            }
        }

        public float XpProgress
        {
            get { return xpProgress; }
            set
            {
                xpProgress = value;
                OnPropertyChanged("XpProgress");
            }
        }

        #endregion

        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }

        private Vector3 position;
        private bool EnablePositionUpdates = true;
        public override Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                // The player entity is remotely controlled, so we
                // don't send it updates when we do physics calculations.
                if (EnablePositionUpdates)
                    OnPropertyChanged("Position");
            }
        }

        public string Username { get; set; }
        /// <summary>
        /// The client's current inventory.
        /// </summary>
        public InventoryWindow Inventory { get; set; }
        public short SelectedSlot
        {
            get { return selectedSlot; }
            set
            {
                selectedSlot = value;
                OnPropertyChanged("SelectedSlot");
            }
        }

        public Slot SelectedItem
        {
            get { return Inventory[SelectedSlot]; }
        }

        /// <summary>
        /// Set to -Vector3.One if the player is not in a bed.
        /// </summary>
        public Vector3 BedPosition
        {
            get { return bedPosition; }
            set
            {
                bedPosition = value;
                OnPropertyChanged("BedPosition");
            }
        }

        public GameMode GameMode
        {
            get { return gameMode; }
            set
            {
                gameMode = value;
                Abilities.FirePropertyChanged = false;
                if (value == GameMode.Creative)
                {
                    Abilities.InstantMine = true;
                    Abilities.Invulnerable = true;
                    Abilities.MayFly = true;
                }
                else
                {
                    Abilities.InstantMine = false;
                    Abilities.Invulnerable = false;
                    Abilities.MayFly = false;
                    Abilities.IsFlying = false;
                }
                Abilities.FirePropertyChanged = true;
                OnPropertyChanged("GameMode");
            }
        }

        public Difficulty Difficulty { get; set; }

        public Vector3 SpawnPoint
        {
            get { return spawnPoint; }
            set
            {
                spawnPoint = value;
                OnPropertyChanged("SpawnPoint");
            }
        }

        public PlayerAbilities Abilities { get; set; }

        /// <summary>
        /// When moving items around in a survival inventory,
        /// this represents the item the player is holding.
        /// </summary>
        public Slot ItemInMouse { get; set; }

        public override bool Invulnerable
        {
            get { return Abilities.Invulnerable; }
        }

        /// <summary>
        /// The position provided by the client.
        /// </summary>
        public Vector3 GivenPosition
        {
            get { return givenPosition; }
            set
            {
                if (PositiveDeltaY > 1.20d && !Abilities.IsFlying)
                {
                    FoodExhaustion += (IsSprinting ? 0.8f : 0.2f);
                    OnJumped();
                }
                givenPosition = value;
                LastGivenPositionUpdate = DateTime.Now;
                OnPropertyChanged("GivenPosition");
            }
        }
        public DateTime LastGivenPositionUpdate { get; set; }
        public bool ShowCape { get; set; }

        public override MetadataDictionary Metadata
        {
            get
            {
                var metadata = base.Metadata;
                metadata[16] = new Metadata.MetadataInt(16, ShowCape ? 1 : 0);
                return metadata;
            }
        }

        /// <summary>
        /// The last entity that attacked the player, used to determine
        /// the killer.
        /// </summary>
        public Entity LastAttackingEntity { get; set; }

        /// <summary>
        /// The type of damage last recieved.
        /// </summary>
        public DamageType LastDamageType { get; set; }

        /// <summary>
        /// The cumulative positive Y delta motion, used to determine if the player
        /// is jumping.
        /// </summary>
        public double PositiveDeltaY { get; set; }

        private Timer bedUseTimer;
        private short food;
        private float foodSaturation;
        private float foodExhaustion;
        private int xpLevel;
        private int xpTotal;
        private float xpProgress;
        private GameMode gameMode;
        private Vector3 bedPosition;
        private short selectedSlot;
        private Vector3 spawnPoint;
        private Vector3 givenPosition;

        public event EventHandler BedStateChanged, BedTimerExpired, StartEating;
        /// <summary>
        /// Note: Only fired when the inventory is changed via SetSlot.
        /// </summary>
        public event EventHandler<InventoryChangedEventArgs> InventoryChanged;
        public event EventHandler<EntityEventArgs> PickUpItem, Jumped;

        #endregion

        public void SetSlot(short index, Slot slot)
        {
            if (InventoryChanged != null)
                InventoryChanged(this, new InventoryChangedEventArgs()
                {
                    Index = index,
                    OldValue = Inventory[index],
                    NewValue = slot
                });
            Inventory[index] = slot;
        }

        public void EnterBed(Vector3 position)
        {
            BedPosition = position;
            bedUseTimer.Change(5000, Timeout.Infinite);
            if (BedStateChanged != null)
                BedStateChanged(this, null);
        }

        public void LeaveBed()
        {
            BedPosition = -Vector3.One;
            if (BedStateChanged != null)
                BedStateChanged(this, null);
        }

        public override void Kill()
        {
            deathTimer.Change(3000, Timeout.Infinite);
            for (int i = 0; i < Inventory.Length; i++)
                Inventory[i] = Slot.EmptySlot;
        }

        protected internal virtual void OnStartEating()
        {
            if (StartEating != null)
                StartEating(this, new EventArgs());
        }

        protected internal virtual void OnJumped()
        {
            if (Jumped != null)
                Jumped(this, new EntityEventArgs(this));
        }

        public override void Damage(int damage, bool accountForArmor = true)
        {
            FoodExhaustion += 0.3f;
            base.Damage(damage, accountForArmor);
        }

        public override void PhysicsUpdate(World world)
        {
            // This doesn't quite work properly, and it becomes a hassle for the user
            return;
            EnablePositionUpdates = false;
            base.PhysicsUpdate(world);
            EnablePositionUpdates = true;
            Velocity *= 0.1f; // We don't get GivenPosition updates often enough to account for slowing down
        }

        public virtual void OnPickUpItem(EntityEventArgs e)
        {
            if (PickUpItem != null)
                PickUpItem(this, e);
        }
    }
}
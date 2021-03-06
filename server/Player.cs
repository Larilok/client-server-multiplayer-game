using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace server
{
    public class Player
    {
        public int id;
        public string username;
        public int health = 100;

        public int damage = 20;

        public float damageMultiplier = 1;
        public float moveSpeedMultiplier = 1;
        public float bulletSpeedMultiplier = 1;
        private const int multiplierDuration = 30000;
        enum MultiplierEnum
        {
            DamageMultiplier,
            MoveSpeedMultiplier,
            BulletSpeedMultiplier
        }

        public Vector3 position;
        public Quaternion rotation;
        public bool invertGunSprite;
        public float gunRotation;

        private float moveSpeed = 20f / Constants.TICKS_PER_SEC;
        private bool[] inputs;
        enum BoostEnum
        {
            HealthBoost,
            PlayerSpeedBoost,
            BulletSpeedBoost,
            BulletDamageBoost
        }

        public Player(int id, string username, Vector3 spawnPosition)
        {
            this.id = id;
            this.username = username;
            //this.bulletList = new List<(Vector3, Vector2)>();
            position = spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            MoveRotate();
        }

        private void MoveRotate()
        {
            Vector2 inputDirection = Vector2.Zero;
            if (inputs[0])
            {
                inputDirection.Y += 1;
            }
            if (inputs[1])
            {
                inputDirection.Y -= 1;
            }
            if (inputs[2])
            {
                inputDirection.X -= 1;
            }
            if (inputs[3])
            {
                inputDirection.X += 1;
            }
            // Vector3 forward = Vector3.Transform(new Vector3(0, 1, 0), rotation);
            // Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));
            Vector3 forward = new Vector3(0, 1, 0);
            Vector3 right = new Vector3(1, 0, 0);


            Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;
            position += moveDirection * moveSpeed * moveSpeedMultiplier;
            if (position.X < -Constants.HORIZONTAL_BORDER) position.X = -Constants.HORIZONTAL_BORDER;
            if (position.X > Constants.HORIZONTAL_BORDER) position.X = Constants.HORIZONTAL_BORDER;
            if (position.Y < -Constants.VERTICAL_BORDER) position.Y = -Constants.VERTICAL_BORDER;
            if (position.Y > Constants.VERTICAL_BORDER) position.Y = Constants.VERTICAL_BORDER;


            //Rotate

            ServerSend.PlayerPosition(this);
        }

        public void SetInput(bool[] inputs, Quaternion rotation)
        {
            this.inputs = inputs;
            this.rotation = rotation;
        }
        public void SetInput(bool[] inputs, bool invert, float aim)
        {
            this.inputs = inputs;
            this.invertGunSprite = invert;
            this.gunRotation = aim;

        }

        internal void AddBoost(int boostId)
        {
            Console.WriteLine($"Adding boost {boostId} to Player {id}");
            if(boostId == (int)BoostEnum.HealthBoost)//Health boost
            {
                if (health <= 40) health += 60;
                ServerSend.PlayerHealthToAll(this);
            } else if (boostId == (int)BoostEnum.PlayerSpeedBoost)
            {
                moveSpeedMultiplier += 0.25f;
                ResetMultiplierDelayed(MultiplierEnum.MoveSpeedMultiplier, multiplierDuration);
            }
            else if(boostId == (int)BoostEnum.BulletSpeedBoost)
            {//HANDLED ON CLIENT
                //bulletSpeedMultiplier += 1f;
                //ResetMultiplierDelayed(MultiplierEnum.BulletSpeedMultiplier, multiplierDuration);
            }
            else if(boostId == (int)BoostEnum.BulletDamageBoost)
            {
                damageMultiplier += 1.5f;
                ResetMultiplierDelayed(MultiplierEnum.DamageMultiplier, multiplierDuration);
            }
        }
        private async void ResetMultiplierDelayed(MultiplierEnum multiplier, int timeout)
        {
            await Task.Delay(timeout);
            Console.WriteLine("Resetting Boost");
            if(multiplier == MultiplierEnum.DamageMultiplier)
            {
                damageMultiplier = 1;
            } else if (multiplier == MultiplierEnum.MoveSpeedMultiplier)
            {
                moveSpeedMultiplier = 1;
            } else //if (multiplier == MultiplierEnum.BulletSpeedMultiplier)
            {
                bulletSpeedMultiplier = 1;
            }
        }

        public void SetHealth(int hitByClientId)
        {
            Player hitByPlayer = Server.clients[hitByClientId].player;
            this.TakeDamage(hitByPlayer.CalculateDamage());
        }
        public bool IsDead() 
        {
            return health <= 0;
        }

        public void TakeDamage(int damage)
        {
            if (health <= 0)
            {
                return;
            }

            health -= damage;
            if (health <= 0)
            {
                health = 0;
            }
        }
        public int CalculateDamage()
        {
            return (int)(this.damage * this.damageMultiplier);
        }
    }
}

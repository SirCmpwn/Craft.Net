﻿namespace Craft.Net.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Craft.Net.Data.Entities;

    using Data;

    public partial class MinecraftClient
    {
        #region public functions

        public void LookAt(Vector3 position)
        {
            var delta = position - new Vector3(this.Position.X, this.Position.Y + 1.62, this.Position.Z);
            var yaw = Math.Atan2(-delta.X, -delta.Z);
            var groundDistance = Math.Sqrt((delta.X * delta.X) + (delta.Z * delta.Z));
            var pitch = Math.Atan2(delta.Y, groundDistance);

            this._yaw = (float)MathHelper.ToNotchianYaw(yaw);
            this._pitch = (float)MathHelper.ToNotchianPitch(pitch);

            SendPacket(new PlayerLookPacket(this.Yaw, this.Pitch, false));
        }

        public Task<Vector3> Move(int distanceX, int distanceZ)
        {
            return Task.Factory.StartNew(() =>
                {
                    var pos = this.Position + new Vector3(distanceX, 0, distanceZ);

                    int xDirection = 1;
                    int zDirection = 1;
                    if (distanceX < 0)
                    {
                        xDirection = -1;
                        distanceX *= -1;
                    }
                    if (distanceZ < 0)
                    {
                        zDirection = -1;
                        distanceZ *= -1;
                    }

                    int maxMove = Math.Max(distanceX, distanceZ);
                    for (int i = 0; i < maxMove; i++)
                    {
                        int newX = 0, newZ = 0;
                        if (i < distanceX) 
                            newX = xDirection;
                        if (i < distanceZ)
                            newZ = zDirection;

                        this.Position += new Vector3(newX, 0, newZ);
                        this.LookAt(pos + new Vector3(0, 1.62, 0));

                        Thread.Sleep(100);
                    }

                    return pos;
                });
        }

        #endregion
    }
}
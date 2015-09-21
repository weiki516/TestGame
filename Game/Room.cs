﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTest.Game
{
    public class Room
    {
        public float lastUpdateTime = 0;
        public int rId;
        public static readonly float gravity = -9.0f;
        public static readonly float deltaTime = 1 / 60f;
        public int frame;
        bool started = false;
        List<Player> players = new List<Player>();
        void AddPlayer(int id)
        {
            players.Add(new Player() { id = id, height = 50 });
            Console.WriteLine("Room {0}: Player {1} enter room at frame {2}", rId, id, frame);
        }

        public Common.EnterRoomReply EnterRoom(int id)
        {
            var ret = new Common.EnterRoomReply { ids = new int[8], count = players.Count};
            for (int i = 0; i < ret.count; i++)
            {
                ret.ids[i] = players[i].id;
            }
            AddPlayer(id);
            return ret;
        }
        public void Update(float time)
        {
            while (started && time - lastUpdateTime > deltaTime)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    var item = players[i];
                    if (!item.dead)
                    {
                        float nextSpeed = item.speed + gravity * deltaTime;
                        float avgSpeed = (item.speed + nextSpeed) / 2;
                        float nextHeight = item.height + avgSpeed * deltaTime;
                        item.speed = nextSpeed;
                        item.height = nextHeight;
                        if (nextHeight > 100 || nextHeight < 0)
                        {
                            item.dead = true;
                            Console.WriteLine("Room {0}: Player {1} is dead as height {2}!", rId, item.id, nextHeight);
                        }
                    }
                }
                lastUpdateTime += deltaTime;
                frame++;
            }
        }

        public Room(int rId, int creatorId)
        {
            this.rId = rId;
            AddPlayer(creatorId);
        }

        public Player GetPlayer(int id)
        {
            return players.Find(x => x.id == id);
        }

        public bool Jump(int id)
        {
            int i = 0;
            for (; i < players.Count; i++)
            {
                var item = players[i];
                if (item.id == id) {
                    if (item.dead)
                        return false;
                    item.Jump();
                    break;
                }
            }
            if (i < players.Count)
            {
                Rpc(i, GameCommand.UserCommandType.Jump, players[i].id);
                return true;
            }
            return false;
        }

        public void Rpc(int expIndex, GameCommand.UserCommandType type, ValueType arg)
        {
            for (int j = 0; j < expIndex; j++)
            {
                var item = players[j];
                item.session.Rpc(type, arg);
            }

            for (int j = expIndex + 1; j < players.Count; j++)
            {
                var item = players[j];
                item.session.Rpc(type, arg);
            }
        }
        public float Start()
        {
            started = true;
            return lastUpdateTime = World.Instance.time;
        }
    }
}

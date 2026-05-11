
using System.Collections.Generic;

namespace game.resource.map
{
    public class Missile
    {
        public class Command
        {
            public enum ID
            {
                unidentified = 0,
                release,
                add,
                clear,
                removeNpc,
            }

            public class Element
            {
                public ID commandID;

                public Element(Command.ID commandID)
                {
                    this.commandID = commandID;
                }
            }

            public class Release : Command.Element
            {
                public Release() : base(Command.ID.release) { }
            }

            public class Add : Command.Element
            {
                public List<settings.skill.Missile> missiles;

                public Add(List<settings.skill.Missile> missiles) : base(Command.ID.add)
                {
                    this.missiles = missiles;
                }
            }

            public class Clear : Command.Element
            {
                public Clear() : base(Command.ID.clear) { }
            }

            public class RemoveNpc : Command.Element
            {
                public settings.npcres.Controller npcController;

                public RemoveNpc(settings.npcres.Controller npcController) : base(Command.ID.removeNpc)
                {
                    this.npcController = npcController;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private readonly System.Threading.Thread mainThreadHandle;
        private readonly Queue<Missile.Command.Element> commandQueue;
        private readonly Dictionary<settings.skill.Missile, bool> missileMapping;

        ////////////////////////////////////////////////////////////////////////////////

        public Missile()
        {
            this.mainThreadHandle = new System.Threading.Thread(this.MainThread);
            this.commandQueue = new Queue<Missile.Command.Element>();
            this.missileMapping = new Dictionary<settings.skill.Missile, bool>();
        }

        public void Initialize()
        {
            this.mainThreadHandle.Start();
        }

        public void Release()
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Missile.Command.Release());
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void Add(List<settings.skill.Missile> missiles)
        {
            if(missiles == null)
            {
                return;
            }

            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Missile.Command.Add(missiles));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void Clear()
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Missile.Command.Clear());
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void RemoveNpc(settings.npcres.Controller npcController)
        {
            if (npcController == null)
            {
                return;
            }

            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Missile.Command.RemoveNpc(npcController));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private void MainThread()
        {
            while (true)
            {
                int continually = 0;
                bool releaseCommanded = false;

                while (true)
                {
                    Missile.Command.Element command = null;

                    lock (this.commandQueue)
                    {
                        if(this.commandQueue.Count > 0)
                        {
                            command = this.commandQueue.Dequeue();
                            continually++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (command.commandID == Command.ID.release)
                    {
                        releaseCommanded = true;
                        break;
                    }

                    switch (command.commandID)
                    {
                        case Command.ID.add:
                            this.Command_Add((map.Missile.Command.Add)command);
                            break;

                        case Command.ID.clear:
                            this.Command_Clear();
                            break;

                        case Command.ID.removeNpc:
                            this.Command_RemoveNpc((map.Missile.Command.RemoveNpc)command);
                            break;

                        default:
                            break;
                    }
                }

                if(releaseCommanded == true)
                {
                    break;
                }

                continually += this.Update_Skill();

                if (continually == 0)
                {
                    lock(commandQueue)
                    {
                        System.Threading.Monitor.Wait(this.commandQueue);
                    }

                    continue;
                }

                System.Threading.Thread.Sleep(55); // 18 fps = 55 milliseconds
            }
        }

        private int Update_Skill()
        {
            int activeCount = 0;
            List<settings.skill.Missile> removeListing = new List<settings.skill.Missile>();

            foreach(KeyValuePair<settings.skill.Missile, bool> pairIndex in this.missileMapping)
            {
                if (pairIndex.Key.Activate() == false)
                {
                    removeListing.Add(pairIndex.Key);
                }

                activeCount++;
            }

            if(removeListing.Count > 0)
            {
                foreach(settings.skill.Missile removeIndex in removeListing)
                {
                    this.missileMapping.Remove(removeIndex);
                }
            }

            return activeCount;
        }

        private void Command_Add(map.Missile.Command.Add command)
        {
            foreach(settings.skill.Missile index in command.missiles)
            {
                this.missileMapping[index] = true;
            }
        }

        private void Command_Clear()
        {
            this.missileMapping.Clear();
        }

        private void Command_RemoveNpc(map.Missile.Command.RemoveNpc command)
        {
            if (command.npcController == null)
            {
                return;
            }

            List<settings.skill.Missile> removeListing = new List<settings.skill.Missile>();
            foreach (KeyValuePair<settings.skill.Missile, bool> pairIndex in this.missileMapping)
            {
                if (pairIndex.Key != null && pairIndex.Key.UsesNpc(command.npcController))
                {
                    removeListing.Add(pairIndex.Key);
                }
            }

            foreach (settings.skill.Missile removeIndex in removeListing)
            {
                this.missileMapping.Remove(removeIndex);
            }
        }
    }
}

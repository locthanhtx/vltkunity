
using System.Collections.Generic;

namespace game.resource.map
{
    public class NpcFrame
    {
        public class Command
        {
            public enum ID
            {
                unidentified = 0,
                release,
                add,
                remove,
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
                public settings.npcres.Controller npcController;

                public Add(settings.npcres.Controller npcController) : base(Command.ID.add)
                {
                    this.npcController = npcController;
                }
            }

            public class Remove : Command.Element
            {
                public settings.npcres.Controller npcController;

                public Remove(settings.npcres.Controller npcController) : base(Command.ID.remove)
                {
                    this.npcController = npcController;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private readonly System.Threading.Thread mainThreadHandle;
        private readonly Queue<NpcFrame.Command.Element> commandQueue;
        private readonly Dictionary<settings.npcres.Controller, bool> npcMapping;

        ////////////////////////////////////////////////////////////////////////////////

        public NpcFrame()
        {
            this.mainThreadHandle = new System.Threading.Thread(this.MainThread);
            this.commandQueue = new Queue<NpcFrame.Command.Element>();
            this.npcMapping = new Dictionary<settings.npcres.Controller, bool>();
        }

        public void Initialize()
        {
            this.mainThreadHandle.Start();
        }

        public void Release()
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new NpcFrame.Command.Release());
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void Add(settings.npcres.Controller npcController)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new NpcFrame.Command.Add(npcController));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void Remove(settings.npcres.Controller npcController)
        {
            if (npcController == null)
            {
                return;
            }

            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new NpcFrame.Command.Remove(npcController));
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
                    NpcFrame.Command.Element command = null;

                    lock (this.commandQueue)
                    {
                        if (this.commandQueue.Count > 0)
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
                            this.Command_Add((map.NpcFrame.Command.Add)command);
                            break;

                        case Command.ID.remove:
                            this.Command_Remove((map.NpcFrame.Command.Remove)command);
                            break;

                        default:
                            break;
                    }
                }

                if (releaseCommanded == true)
                {
                    break;
                }

                continually += this.Update_Frame();

                if (continually == 0)
                {
                    lock (commandQueue)
                    {
                        System.Threading.Monitor.Wait(this.commandQueue);
                    }

                    continue;
                }

                System.Threading.Thread.Sleep(55); // 18 fps = 55 milliseconds
            }
        }

        private int Update_Frame()
        {
            foreach(KeyValuePair<settings.npcres.Controller, bool> pairIndex in this.npcMapping)
            {
                pairIndex.Key.Activate();
            }

            return 1;
        }

        private void Command_Add(map.NpcFrame.Command.Add command)
        {
            this.npcMapping[command.npcController] = true;
        }

        private void Command_Remove(map.NpcFrame.Command.Remove command)
        {
            if (command.npcController != null)
            {
                this.npcMapping.Remove(command.npcController);
            }
        }
    }
}

namespace GraphX.PCL.Logic.Helpers
{
    /* public partial class Form1
        {
            List<Thd> Thds = new List<Thd>();
            ListView LV = new ListView();
            protected override void OnLoad(System.EventArgs e)
            {
                base.OnLoad(e);
                LV.View = View.Details;
                LV.GridLines = true;
                LV.Parent = this;
                LV.Columns.Add("Processor");
                LV.Columns.Add("TimePerPass");
                LV.CheckBoxes = true;
                int I = 0;
                for (I = 0; I <= Environment.ProcessorCount - 1; I++)
                {
                    int Processor = I;
                    LV.Items.Add(I.ToString());
                    LV.Items[I].SubItems.Add("");
                    Thds.Add(new Thd(I));
                    Thds[I].Done += Thds_Done;
                    Thds[I].UpdateTime += Thds_UpdateTime;
                }
                this.ClientSize = new Size(200, 200);
                LV.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                LV.Dock = DockStyle.Fill;
                LV.ItemCheck += LV_ItemCheck;
            }
            public void LV_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    Thds[e.Index].PauseThd = false;
                }
                else
                {
                    Thds[e.Index].PauseThd = true;
                }
            }
            protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
            {
                base.OnFormClosing(e);
                foreach (Thd T in Thds)
                {
                    if (!T.StopThd)
                    {
                        T.StopThd = true;
                        e.Cancel = true;
                    }
                }
            }
            public delegate void CloseDelegate();
            public delegate void UpdateTimeDelgate(int Index, long T);
            public void Thds_Done(object Sender, EventArgs e)
            {
                Thds.Remove((Thd)Sender);
                if (Thds.Count == 0)
                {
                    this.Invoke(new CloseDelegate(this.Close));
                }
            }
            public void Thds_UpdateTime(object sender, long IterationTime)
            {
                int Index = Thds.IndexOf((Thd)sender);
                this.LV.Invoke(new UpdateTimeDelgate(UpdateTime), new object[] {
				Index,
				IterationTime
			});
            }
            public void UpdateTime(int Index, long T)
            {
                this.LV.Items[Index].SubItems[1].Text = T.ToString();
            }

        }
    }*/

    /*class Thd
    {
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int GetCurrentThreadId();
        public event DoneEventHandler Done;
        public delegate void DoneEventHandler(object sender, EventArgs e);
        public event UpdateTimeEventHandler UpdateTime;
        public delegate void UpdateTimeEventHandler(object sender, long IterationTime);
        Thread Thrd;
        ProcessThread NativeThd;
        int Processor;
        internal bool PauseThd = true;
        internal bool StopThd;
        const int Iterations = 99999999;
        public Thd(int Processor)
        {
            this.Processor = Processor;
            this.Thrd = new Thread(Start);
            this.Thrd.Start();
        }
        public void Start()
        {
            Thread.BeginThreadAffinity();
            Random R = new Random();
            ProcessThreadCollection PTs = Process.GetCurrentProcess().Threads;
            int PTId = GetCurrentThreadId();
            int I = 0;
            int J = 0;
            int N = 0;
            for (I = 0; I <= PTs.Count - 1; I++)
            {
                if (PTs[I].Id == PTId)
                    break; // TODO: might not be correct. Was : Exit For
            }
            if (I < PTs.Count)
            {
                NativeThd = PTs[I];
                NativeThd.ProcessorAffinity = new IntPtr(1 << Processor);
            }
            else
            {
                //MessageBox.Show("Thread not found");
            }
            for (J = 1; J <= int.MaxValue - 1; J++)
            {
                if (StopThd)
                    break; // TODO: might not be correct. Was : Exit For
                if (PauseThd)
                {
                    if (UpdateTime != null)
                    {
                        UpdateTime(this, 0);
                    }
                    while (PauseThd)
                    {
                        if (StopThd)
                            break; // TODO: might not be correct. Was : Exit For
                        Thread.Sleep(100);
                    }
                }
                Stopwatch SW = Stopwatch.StartNew();
                for (I = 0; I <= Iterations; I++)
                {
                    N = R.Next();
                }
                SW.Stop();
                if (UpdateTime != null)
                {
                    UpdateTime(this, SW.ElapsedMilliseconds);
                }
            }
            Thread.EndThreadAffinity();
            if (Done != null)
            {
                Done(this, null);
            }
        }
    }*/
    

}

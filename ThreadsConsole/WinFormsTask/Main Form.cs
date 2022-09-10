namespace WinFormsTask
{
    public partial class Form1 : Form
    {
        int j = 0;
        private object thisLock = new object();
        private CancellationTokenSource ctSource;
        private CancellationToken token;  // треба дл€ скасуванн€ любого потока €кий захочем

        public Form1()
        {
            InitializeComponent();
        }

        private void btnPerform_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtNumber.Text)) return;
            
            try
            {
                int count = int.Parse(txtNumber.Text);
                ctSource = new CancellationTokenSource();
                token = ctSource.Token;
                btnPerform.Enabled = false;
                Task task = new Task(() => CopyData(count), token /*вказуЇм токен дл€ в≥дм≥ни. який може в≥дм≥нити цей пот≥к*/);
                task.Start();
                //Task.Run(() => CopyData(count));
            }
            catch
            {
                return;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            ctSource.Cancel(); // в≥дм≥н€Ї роботу потоку
        }

        void CopyData(int count)
        {
            lock (thisLock) // ÷е lock крч. якщо “аски починають конфл≥кн≥чать м≥ж собою, то один пропускаЇ ≥нший таск вперед а пот≥м сам йде. ÷им самим уникаютьс€ конфл≥кти
            {
                for (int i = 0; i < count; i++)
                {
                    if (token.IsCancellationRequested) // провер€ем наличие сигнала отмены задачи
                    {
                        btnPerform.Enabled = true;
                        j = 0;
                        lbCounter.Text = "0/0";
                        return; // выходим из метода и тем самым завершаем задачу
                    }
                    Thread.Sleep(500);
                    if (lbCounter.InvokeRequired)
                    {
                        string text = $"{++j}/{count}";
                        lbCounter.Invoke(new MethodInvoker(delegate { lbCounter.Text = text; }));
                    }
                }
                if (btnPerform.InvokeRequired)
                {
                    btnPerform.Invoke(new MethodInvoker(delegate { btnPerform.Enabled = true; }));
                }
            }
            
        }
        
    }
}
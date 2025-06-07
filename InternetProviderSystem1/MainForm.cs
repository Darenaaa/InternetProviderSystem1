using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace InternetProviderSystem
{
    //  АБСТРАКЦІЯ 
    public abstract class Service
    {
        public abstract string GetServiceName();
        public abstract decimal GetPrice();
        public abstract string GetDescription();
    }

    public class InternetService : Service
    {
        private int speed;
        public InternetService(int speed) { this.speed = speed; }
        public override string GetServiceName() => "Інтернет";
        public override decimal GetPrice() => speed * 2.5m;
        public override string GetDescription() => $"Швидкість: {speed} Мбіт/с";
    }

    public class TVService : Service
    {
        private int channels;
        public TVService(int channels) { this.channels = channels; }
        public override string GetServiceName() => "Кабельне ТБ";
        public override decimal GetPrice() => channels * 1.5m;
        public override string GetDescription() => $"Каналів: {channels}";
    }

    public class PhoneService : Service
    {
        private int minutes;
        public PhoneService(int minutes) { this.minutes = minutes; }
        public override string GetServiceName() => "Телефонія";
        public override decimal GetPrice() => minutes * 0.8m;
        public override string GetDescription() => $"Хвилин: {minutes}";
    }

    // ПОЛІМОРФІЗМ 
    public interface ITariff
    {
        decimal CalculatePrice(int value);
        string GetTariffName();
    }

    public class HourlyTariff : ITariff
    {
        public decimal CalculatePrice(int hours) => hours * 15m;
        public string GetTariffName() => "Погодинний (15 грн/год)";
    }

    public class FixedTariff : ITariff
    {
        private string name;
        private decimal price;
        public FixedTariff(string name, decimal price) { this.name = name; this.price = price; }
        public decimal CalculatePrice(int value) => price;
        public string GetTariffName() => $"{name} ({price} грн/міс)";
    }

    // ІНКАПСУЛЯЦІЯ 
    public class PaymentRecord
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }

    public class Client
    {
        private string _name;
        private string _email;
        private List<PaymentRecord> _paymentHistory;
        private List<Service> _services;
        private decimal _balance;
        private bool _isActive;

        public Client(string name, string email)
        {
            _name = name ?? "";
            _email = email ?? "";
            _paymentHistory = new List<PaymentRecord>();
            _services = new List<Service>();
            _balance = 0;
            _isActive = true;
        }

        // Публічні методи доступу (інкапсуляція)
        public string GetName() => _name;
        public string GetEmail() => _email;
        public List<PaymentRecord> GetPaymentHistory() => _paymentHistory ?? new List<PaymentRecord>();
        public List<Service> GetServices() => _services ?? new List<Service>();
        public decimal GetBalance() => _balance;
        public bool IsActive() => _isActive;
        public virtual decimal GetDiscount() => 0m;

        public void SetName(string name) => _name = name ?? "";
        public void SetEmail(string email) => _email = email ?? "";
        public void SetActive(bool active) => _isActive = active;
        public void AddPayment(PaymentRecord payment)
        {
            if (payment != null)
            {
                _paymentHistory.Add(payment);
                _balance += payment.Amount;
            }
        }
        public void AddService(Service service)
        {
            if (service != null)
                _services.Add(service);
        }
        public void RemoveService(Service service)
        {
            if (service != null)
                _services.Remove(service);
        }
    }

    //  НАСЛІДУВАННЯ 
    public class HomeUser : Client
    {
        public HomeUser(string name, string email) : base(name, email) { }
        public override decimal GetDiscount() => 5m; // 5% знижка
    }

    public class BusinessUser : Client
    {
        public BusinessUser(string name, string email) : base(name, email) { }
        public override decimal GetDiscount() => 15m; // 15% знижка
    }

    public class VIPUser : Client
    {
        public VIPUser(string name, string email) : base(name, email) { }
        public override decimal GetDiscount() => 25m; // 25% знижка
    }

    // ГОЛОВНА ФОРМА 
    public partial class MainForm : Form
    {
        private List<Client> clients;
        private List<ITariff> tariffs;
        private ListView clientListView;
        private Timer statsTimer;
        private Label statsLabel;
        private ComboBox tariffComboBox;
        private Label resultLabel;

        public MainForm()
        {
            // Ініціалізація колекцій перед усім іншим
            clients = new List<Client>();
            tariffs = new List<ITariff>();

            InitializeTariffs();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Налаштування форми
            this.Text = "CyberNet ISP - Система керування провайдером";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 15, 35);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1200, 800);

            // Створення інтерфейсу
            CreateMainInterface();
        }

        private void CreateMainInterface()
        {
            // Головна панель
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            this.Controls.Add(mainPanel);

            // Заголовок
            Label titleLabel = new Label
            {
                Text = "CyberNet ISP Management System",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 255, 255),
                Location = new Point(50, 30),
                Size = new Size(600, 40),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(titleLabel);

            // Панель клієнтів
            Panel clientPanel = CreateStyledPanel("Управління клієнтами", 50, 100, 400, 500);
            CreateClientButtons(clientPanel);
            mainPanel.Controls.Add(clientPanel);

            // Панель тарифів
            Panel tariffPanel = CreateStyledPanel("Тарифи та розрахунки", 480, 100, 400, 300);
            CreateTariffControls(tariffPanel);
            mainPanel.Controls.Add(tariffPanel);

            // Панель статистики
            Panel statsPanel = CreateStyledPanel("Статистика", 910, 100, 400, 400);
            CreateStatsControls(statsPanel);
            mainPanel.Controls.Add(statsPanel);

            // Панель швидких дій
            Panel quickPanel = CreateStyledPanel("Швидкі дії", 480, 430, 400, 170);
            CreateQuickButtons(quickPanel);
            mainPanel.Controls.Add(quickPanel);

            // Список клієнтів
            CreateClientListView(mainPanel);
        }

        private Panel CreateStyledPanel(string title, int x, int y, int width, int height)
        {
            Panel panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(80, 25, 25, 45)
            };

            // Заголовок панелі
            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 255, 255),
                Location = new Point(10, 10),
                Size = new Size(width - 20, 25),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(titleLabel);

            // Кастомна відрисовка з заокругленими краями
            panel.Paint += (s, e) =>
            {
                try
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 15;
                        path.AddArc(0, 0, radius, radius, 180, 90);
                        path.AddArc(width - radius, 0, radius, radius, 270, 90);
                        path.AddArc(width - radius, height - radius, radius, radius, 0, 90);
                        path.AddArc(0, height - radius, radius, radius, 90, 90);
                        path.CloseAllFigures();

                        using (LinearGradientBrush brush = new LinearGradientBrush(
                            new Rectangle(0, 0, width, height),
                            Color.FromArgb(100, 45, 45, 85),
                            Color.FromArgb(150, 25, 25, 55),
                            LinearGradientMode.Vertical))
                        {
                            e.Graphics.FillPath(brush, path);
                        }

                        using (Pen pen = new Pen(Color.FromArgb(100, 0, 255, 255), 2))
                        {
                            e.Graphics.DrawPath(pen, path);
                        }
                    }
                }
                catch
                {
                    // Fallback у випадку помилки малювання
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(80, 25, 25, 45)),
                        new Rectangle(0, 0, width, height));
                }
            };

            return panel;
        }

        private Button CreateStyledButton(string text, Color backColor, Point location, Size size)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255);

            // Hover ефекти без анімації
            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.FromArgb(Math.Min(255, backColor.R + 30),
                                             Math.Min(255, backColor.G + 30),
                                             Math.Min(255, backColor.B + 30));
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = backColor;
            };

            return btn;
        }

        private void CreateClientButtons(Panel panel)
        {
            var btnAddClient = CreateStyledButton("+ Додати клієнта", Color.FromArgb(0, 150, 100), new Point(20, 50), new Size(150, 35));
            btnAddClient.Click += (s, e) => ShowAddClientDialog();
            panel.Controls.Add(btnAddClient);

            var btnEditClient = CreateStyledButton("Редагувати", Color.FromArgb(100, 150, 0), new Point(200, 50), new Size(120, 35));
            btnEditClient.Click += (s, e) => EditSelectedClient();
            panel.Controls.Add(btnEditClient);

            var btnActivateClient = CreateStyledButton("Активувати", Color.FromArgb(0, 100, 200), new Point(20, 100), new Size(120, 35));
            btnActivateClient.Click += (s, e) => ToggleClientStatus(true);
            panel.Controls.Add(btnActivateClient);

            var btnDeactivateClient = CreateStyledButton("Деактивувати", Color.FromArgb(200, 100, 0), new Point(200, 100), new Size(120, 35));
            btnDeactivateClient.Click += (s, e) => ToggleClientStatus(false);
            panel.Controls.Add(btnDeactivateClient);

            var btnAddService = CreateStyledButton("+ Послуга", Color.FromArgb(150, 0, 150), new Point(20, 150), new Size(120, 35));
            btnAddService.Click += (s, e) => ShowAddServiceDialog();
            panel.Controls.Add(btnAddService);

            var btnRemoveService = CreateStyledButton("- Послуга", Color.FromArgb(200, 50, 50), new Point(200, 150), new Size(120, 35));
            btnRemoveService.Click += (s, e) => RemoveSelectedService();
            panel.Controls.Add(btnRemoveService);

            var btnPayment = CreateStyledButton("Платіж", Color.FromArgb(0, 200, 0), new Point(20, 200), new Size(120, 35));
            btnPayment.Click += (s, e) => ShowPaymentDialog();
            panel.Controls.Add(btnPayment);

            var btnBonus = CreateStyledButton("Бонус", Color.FromArgb(255, 165, 0), new Point(200, 200), new Size(120, 35));
            btnBonus.Click += (s, e) => ShowBonusDialog();
            panel.Controls.Add(btnBonus);

            var btnReport = CreateStyledButton("Звіт клієнта", Color.FromArgb(100, 100, 200), new Point(20, 250), new Size(150, 35));
            btnReport.Click += (s, e) => ShowClientReport();
            panel.Controls.Add(btnReport);

            var btnDelete = CreateStyledButton("Видалити", Color.FromArgb(200, 0, 0), new Point(200, 250), new Size(120, 35));
            btnDelete.Click += (s, e) => DeleteSelectedClient();
            panel.Controls.Add(btnDelete);
        }

        private void CreateTariffControls(Panel panel)
        {
            Label lblTariff = new Label
            {
                Text = "Оберіть тариф:",
                Location = new Point(20, 50),
                Size = new Size(120, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblTariff);

            tariffComboBox = new ComboBox
            {
                Location = new Point(20, 75),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 85),
                ForeColor = Color.White
            };

            if (tariffs != null)
            {
                foreach (var tariff in tariffs)
                {
                    tariffComboBox.Items.Add(tariff.GetTariffName());
                }
                if (tariffComboBox.Items.Count > 0)
                    tariffComboBox.SelectedIndex = 0;
            }
            panel.Controls.Add(tariffComboBox);

            var btnCalculate = CreateStyledButton("Розрахувати", Color.FromArgb(0, 150, 200), new Point(20, 120), new Size(120, 35));
            btnCalculate.Click += (s, e) => ShowCalculator();
            panel.Controls.Add(btnCalculate);

            var btnNewTariff = CreateStyledButton("Новий тариф", Color.FromArgb(150, 100, 0), new Point(200, 120), new Size(120, 35));
            btnNewTariff.Click += (s, e) => CreateNewTariff();
            panel.Controls.Add(btnNewTariff);

            resultLabel = new Label
            {
                Text = "Результат: 0 грн",
                Location = new Point(20, 170),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(0, 255, 255),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(resultLabel);

            // Автоматичний розрахунок при зміні тарифу
            tariffComboBox.SelectedIndexChanged += (s, e) =>
            {
                try
                {
                    if (tariffComboBox.SelectedIndex >= 0 && tariffs != null && tariffs.Count > tariffComboBox.SelectedIndex)
                    {
                        var selectedTariff = tariffs[tariffComboBox.SelectedIndex];
                        decimal price = selectedTariff.CalculatePrice(1);
                        if (resultLabel != null)
                            resultLabel.Text = $"Результат: {price} грн";
                    }
                }
                catch
                {
                    if (resultLabel != null)
                        resultLabel.Text = "Результат: 0 грн";
                }
            };
        }

        private void CreateStatsControls(Panel panel)
        {
            statsLabel = new Label
            {
                Text = "",
                Location = new Point(20, 50),
                Size = new Size(350, 300),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 9)
            };
            panel.Controls.Add(statsLabel);

            // Таймер для оновлення статистики
            statsTimer = new Timer { Interval = 3000 };
            statsTimer.Tick += (s, e) => UpdateStats();
            statsTimer.Start();

            UpdateStats();
        }

        private void CreateQuickButtons(Panel panel)
        {
            var btnMassPayment = CreateStyledButton("Масові платежі", Color.FromArgb(100, 0, 200), new Point(20, 50), new Size(120, 35));
            btnMassPayment.Click += (s, e) => ShowMassPayment();
            panel.Controls.Add(btnMassPayment);

            var btnPromo = CreateStyledButton("Акції", Color.FromArgb(200, 100, 0), new Point(200, 50), new Size(120, 35));
            btnPromo.Click += (s, e) => {
                try
                {
                    Form promoForm = new Form
                    {
                        Text = "🎉 Акції та знижки",
                        Size = new Size(500, 400),
                        StartPosition = FormStartPosition.CenterParent,
                        BackColor = Color.FromArgb(45, 45, 85),
                        ForeColor = Color.White,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };

                    ListBox lstPromos = new ListBox
                    {
                        Location = new Point(20, 50),
                        Size = new Size(440, 200),
                        BackColor = Color.FromArgb(25, 25, 45),
                        ForeColor = Color.White,
                        Font = new Font("Arial", 10)
                    };
                    lstPromos.Items.AddRange(new[]
                    {
                        "🔥 -30% на всі тарифи для нових клієнтів",
                        "💎 VIP статус безкоштовно на 3 місяці",
                        "📺 +50 каналів безкоштовно до інтернету",
                        "📞 Безлімітні дзвінки протягом року",
                        "🚀 Швидкість х2 за стару ціну"
                    });

                    Button btnApply = new Button
                    {
                        Text = "Застосувати акцію",
                        Location = new Point(150, 270),
                        Size = new Size(150, 35),
                        BackColor = Color.FromArgb(200, 100, 0),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnApply.Click += (ss, ee) => {
                        MessageBox.Show("Акцію застосовано до обраних клієнтів!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };

                    Button btnClose = new Button
                    {
                        Text = "Закрити",
                        Location = new Point(320, 270),
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(150, 50, 50),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnClose.Click += (ss, ee) => promoForm.Close();

                    Label lblTitle = new Label
                    {
                        Text = "Поточні акції та спеціальні пропозиції:",
                        Location = new Point(20, 20),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 12, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 255, 255)
                    };

                    promoForm.Controls.AddRange(new Control[] { lblTitle, lstPromos, btnApply, btnClose });
                    promoForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка модуля акцій: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            panel.Controls.Add(btnPromo);

            var btnSupport = CreateStyledButton("Техпідтримка", Color.FromArgb(0, 200, 100), new Point(20, 100), new Size(120, 35));
            btnSupport.Click += (s, e) => {
                try
                {
                    Form supportForm = new Form
                    {
                        Text = "🎧 Техпідтримка",
                        Size = new Size(600, 450),
                        StartPosition = FormStartPosition.CenterParent,
                        BackColor = Color.FromArgb(45, 45, 85),
                        ForeColor = Color.White,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };

                    ListView lstTickets = new ListView
                    {
                        Location = new Point(20, 50),
                        Size = new Size(540, 200),
                        View = View.Details,
                        FullRowSelect = true,
                        GridLines = true,
                        BackColor = Color.FromArgb(25, 25, 45),
                        ForeColor = Color.White
                    };
                    lstTickets.Columns.Add("ID", 50);
                    lstTickets.Columns.Add("Клієнт", 150);
                    lstTickets.Columns.Add("Проблема", 200);
                    lstTickets.Columns.Add("Статус", 80);
                    lstTickets.Columns.Add("Дата", 80);

                    lstTickets.Items.Add(new ListViewItem(new[] { "001", "Іван Петров", "Немає інтернету", "Відкрито", "31.05.25" }));
                    lstTickets.Items.Add(new ListViewItem(new[] { "002", "Марія Коваль", "Повільна швидкість", "В роботі", "31.05.25" }));
                    lstTickets.Items.Add(new ListViewItem(new[] { "003", "Олег Сидоров", "Проблеми з ТВ", "Закрито", "30.05.25" }));

                    Button btnCreate = new Button
                    {
                        Text = "Нова заявка",
                        Location = new Point(20, 270),
                        Size = new Size(120, 35),
                        BackColor = Color.FromArgb(0, 200, 100),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnCreate.Click += (ss, ee) => {
                        string id = (lstTickets.Items.Count + 1).ToString("000");
                        lstTickets.Items.Add(new ListViewItem(new[] { id, "Новий клієнт", "Нова проблема", "Відкрито", DateTime.Now.ToString("dd.MM.yy") }));
                        MessageBox.Show("Заявку створено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };

                    Button btnClose = new Button
                    {
                        Text = "Закрити",
                        Location = new Point(440, 270),
                        Size = new Size(120, 35),
                        BackColor = Color.FromArgb(150, 50, 50),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnClose.Click += (ss, ee) => supportForm.Close();

                    Label lblTitle = new Label
                    {
                        Text = "Заявки техпідтримки:",
                        Location = new Point(20, 20),
                        Size = new Size(300, 25),
                        Font = new Font("Arial", 12, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 255, 255)
                    };

                    supportForm.Controls.AddRange(new Control[] { lblTitle, lstTickets, btnCreate, btnClose });
                    supportForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка техпідтримки: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            panel.Controls.Add(btnSupport);

            var btnSettings = CreateStyledButton("Налаштування", Color.FromArgb(150, 150, 0), new Point(200, 100), new Size(120, 35));
            btnSettings.Click += (s, e) => {
                try
                {
                    Form settingsForm = new Form
                    {
                        Text = "⚙️ Налаштування системи",
                        Size = new Size(500, 450),
                        StartPosition = FormStartPosition.CenterParent,
                        BackColor = Color.FromArgb(45, 45, 85),
                        ForeColor = Color.White,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };

                    GroupBox grpGeneral = new GroupBox
                    {
                        Text = "Загальні налаштування",
                        Location = new Point(20, 20),
                        Size = new Size(440, 120),
                        ForeColor = Color.FromArgb(0, 255, 255)
                    };

                    CheckBox chkAutoBackup = new CheckBox
                    {
                        Text = "✅ Автоматичне резервне копіювання",
                        Location = new Point(20, 30),
                        Size = new Size(350, 20),
                        ForeColor = Color.White,
                        Checked = true
                    };

                    CheckBox chkNotifications = new CheckBox
                    {
                        Text = "🔔 Сповіщення про нові платежі",
                        Location = new Point(20, 55),
                        Size = new Size(350, 20),
                        ForeColor = Color.White,
                        Checked = true
                    };

                    grpGeneral.Controls.AddRange(new Control[] { chkAutoBackup, chkNotifications });

                    GroupBox grpSecurity = new GroupBox
                    {
                        Text = "Безпека",
                        Location = new Point(20, 160),
                        Size = new Size(440, 120),
                        ForeColor = Color.FromArgb(0, 255, 255)
                    };

                    Label lblInfo = new Label
                    {
                        Text = "🔒 Шифрування даних: АКТИВНО\n📋 Журнал аудиту: АКТИВНО\n🛡️ Двофакторна автентифікація: АКТИВНО",
                        Location = new Point(20, 30),
                        Size = new Size(400, 80),
                        ForeColor = Color.White,
                        Font = new Font("Arial", 9)
                    };

                    grpSecurity.Controls.Add(lblInfo);

                    Button btnSave = new Button
                    {
                        Text = "Зберегти",
                        Location = new Point(280, 350),
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(0, 150, 100),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnSave.Click += (ss, ee) => {
                        MessageBox.Show("Налаштування збережено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        settingsForm.Close();
                    };

                    Button btnCancel = new Button
                    {
                        Text = "Скасувати",
                        Location = new Point(390, 350),
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(150, 50, 50),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnCancel.Click += (ss, ee) => settingsForm.Close();

                    settingsForm.Controls.AddRange(new Control[] { grpGeneral, grpSecurity, btnSave, btnCancel });
                    settingsForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка налаштувань: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            panel.Controls.Add(btnSettings);
        }

        private void CreateClientListView(Panel parent)
        {
            clientListView = new ListView
            {
                Location = new Point(50, 630),
                Size = new Size(1260, 200),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(45, 45, 85),
                ForeColor = Color.White
            };

            clientListView.Columns.Add("Ім'я", 150);
            clientListView.Columns.Add("Email", 200);
            clientListView.Columns.Add("Тип", 100);
            clientListView.Columns.Add("Статус", 80);
            clientListView.Columns.Add("Баланс", 100);
            clientListView.Columns.Add("Послуги", 300);
            clientListView.Columns.Add("Знижка", 80);
            clientListView.Columns.Add("Платежів", 80);

            parent.Controls.Add(clientListView);
            UpdateClientList();
        }

        private void InitializeTariffs()
        {
            if (tariffs == null)
                tariffs = new List<ITariff>();

            tariffs.Clear();
            tariffs.Add(new HourlyTariff());
            tariffs.Add(new FixedTariff("Базовий план", 299));
            tariffs.Add(new FixedTariff("Стандартний план", 499));
            tariffs.Add(new FixedTariff("Преміум план", 799));
            tariffs.Add(new FixedTariff("Корпоративний план", 1299));
            tariffs.Add(new FixedTariff("VIP план", 1999));
        }

        private void UpdateClientList()
        {
            try
            {
                if (clientListView == null || clients == null) return;

                clientListView.Items.Clear();
                foreach (var client in clients)
                {
                    if (client == null) continue;

                    var services = client.GetServices() ?? new List<Service>();
                    string serviceNames = string.Join(", ", services.Select(s => s?.GetServiceName() ?? "Невідомо"));

                    ListViewItem item = new ListViewItem(new string[]
                    {
                        client.GetName() ?? "",
                        client.GetEmail() ?? "",
                        client.GetType().Name.Replace("User", ""),
                        client.IsActive() ? "Активний" : "Неактивний",
                        $"{client.GetBalance():F2} грн",
                        serviceNames,
                        $"{client.GetDiscount()}%",
                        client.GetPaymentHistory()?.Count.ToString() ?? "0"
                    });
                    clientListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення списку: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateStats()
        {
            try
            {
                if (statsLabel == null || clients == null) return;

                int homeUsers = clients.OfType<HomeUser>().Count();
                int businessUsers = clients.OfType<BusinessUser>().Count();
                int vipUsers = clients.OfType<VIPUser>().Count();
                int activeClients = clients.Count(c => c?.IsActive() == true);

                decimal totalRevenue = 0;
                decimal totalBalance = 0;
                int internetCount = 0, tvCount = 0, phoneCount = 0;

                foreach (var client in clients)
                {
                    if (client == null) continue;

                    var payments = client.GetPaymentHistory();
                    if (payments != null)
                        totalRevenue += payments.Sum(p => p?.Amount ?? 0);

                    totalBalance += client.GetBalance();

                    var services = client.GetServices();
                    if (services != null)
                    {
                        internetCount += services.OfType<InternetService>().Count();
                        tvCount += services.OfType<TVService>().Count();
                        phoneCount += services.OfType<PhoneService>().Count();
                    }
                }

                decimal avgBalance = clients.Count > 0 ? totalBalance / clients.Count : 0;

                statsLabel.Text = $@"Загальна статистика:

Клієнти за типами:
🏠 Домашні: {homeUsers}
🏢 Бізнес: {businessUsers}
👑 VIP: {vipUsers}

Стан клієнтів:
✅ Активні: {activeClients}
❌ Неактивні: {clients.Count - activeClients}

Фінанси:
💰 Загальний дохід: {totalRevenue:F2} грн
📊 Середній баланс: {avgBalance:F2} грн
🎯 Всього клієнтів: {clients.Count}

Послуги:
🌐 Інтернет: {internetCount}
📺 ТВ: {tvCount}
📞 Телефонія: {phoneCount}";
            }
            catch (Exception ex)
            {
                if (statsLabel != null)
                    statsLabel.Text = $"Помилка оновлення статистики: {ex.Message}";
            }
        }

        // Методи для обробки подій
        private void ShowAddClientDialog()
        {
            try
            {
                using (var dialog = new AddClientDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.CreatedClient != null)
                    {
                        clients.Add(dialog.CreatedClient);
                        UpdateClientList();
                        MessageBox.Show("Клієнта успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання клієнта: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedClient()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count)
                    {
                        var client = clients[index];
                        if (client != null)
                        {
                            string newName = ShowSimpleInput("Редагування клієнта", "Нове ім'я:", client.GetName());
                            if (!string.IsNullOrEmpty(newName))
                            {
                                client.SetName(newName);
                                UpdateClientList();
                                MessageBox.Show("Клієнт успішно відредагований!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта для редагування!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка редагування: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleClientStatus(bool active)
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count && clients[index] != null)
                    {
                        clients[index].SetActive(active);
                        UpdateClientList();
                        MessageBox.Show($"Клієнт {(active ? "активований" : "деактивований")}!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка зміни статусу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAddServiceDialog()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count && clients[index] != null)
                    {
                        using (var dialog = new AddServiceDialog())
                        {
                            if (dialog.ShowDialog() == DialogResult.OK && dialog.CreatedService != null)
                            {
                                clients[index].AddService(dialog.CreatedService);
                                UpdateClientList();
                                MessageBox.Show("Послугу успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта для додавання послуги!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання послуги: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveSelectedService()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count && clients[index] != null)
                    {
                        var services = clients[index].GetServices();
                        if (services?.Count > 0)
                        {
                            services.RemoveAt(0);
                            UpdateClientList();
                            MessageBox.Show("Послугу видалено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("У клієнта немає послуг для видалення!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення послуги: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPaymentDialog()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count && clients[index] != null)
                    {
                        using (var dialog = new PaymentDialog())
                        {
                            if (dialog.ShowDialog() == DialogResult.OK && dialog.Payment != null)
                            {
                                clients[index].AddPayment(dialog.Payment);
                                UpdateClientList();
                                MessageBox.Show("Платіж успішно проведено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта для платежу!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка платежу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowBonusDialog()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count && clients[index] != null)
                    {
                        string amountStr = ShowSimpleInput("Бонус", "Сума бонусу (грн):", "100");
                        if (!string.IsNullOrEmpty(amountStr) && decimal.TryParse(amountStr, out decimal amount) && amount > 0)
                        {
                            var payment = new PaymentRecord
                            {
                                Amount = amount,
                                Date = DateTime.Now,
                                Description = "Бонусне нарахування"
                            };
                            clients[index].AddPayment(payment);
                            UpdateClientList();
                            MessageBox.Show($"Бонус {amount} грн нараховано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка нарахування бонусу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowClientReport()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    int index = clientListView.SelectedItems[0].Index;
                    if (index >= 0 && index < clients.Count && clients[index] != null)
                    {
                        var client = clients[index];
                        var services = client.GetServices() ?? new List<Service>();
                        var payments = client.GetPaymentHistory() ?? new List<PaymentRecord>();

                        string report = $@"Звіт по клієнту: {client.GetName()}
Email: {client.GetEmail()}
Тип: {client.GetType().Name.Replace("User", "")}
Статус: {(client.IsActive() ? "Активний" : "Неактивний")}
Знижка: {client.GetDiscount()}%
Баланс: {client.GetBalance():F2} грн

Послуги:
{(services.Any() ? string.Join("\n", services.Select(s => $"- {s?.GetServiceName()}: {s?.GetDescription()} - {s?.GetPrice():F2} грн")) : "Немає послуг")}

Історія платежів:
{(payments.Any() ? string.Join("\n", payments.Select(p => $"- {p?.Date:dd.MM.yyyy}: {p?.Amount:F2} грн - {p?.Description}")) : "Немає платежів")}";

                        MessageBox.Show(report, "Звіт клієнта", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення звіту: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedClient()
        {
            try
            {
                if (clientListView?.SelectedItems?.Count > 0)
                {
                    if (MessageBox.Show("Ви впевнені, що хочете видалити клієнта?", "Підтвердження",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int index = clientListView.SelectedItems[0].Index;
                        if (index >= 0 && index < clients.Count)
                        {
                            clients.RemoveAt(index);
                            UpdateClientList();
                            MessageBox.Show("Клієнт видалений!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть клієнта!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowMassPayment()
        {
            try
            {
                if (clients?.Count > 0)
                {
                    using (var dialog = new MassPaymentDialog(clients))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            UpdateClientList();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Немає клієнтів для масових платежів!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка масових платежів: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowCalculator()
        {
            try
            {
                if (tariffs?.Count > 0)
                {
                    using (var dialog = new TariffCalculatorForm(tariffs))
                    {
                        dialog.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("Немає доступних тарифів!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка калькулятора: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateNewTariff()
        {
            try
            {
                string name = ShowSimpleInput("Новий тариф", "Назва тарифу:", "");
                if (!string.IsNullOrEmpty(name))
                {
                    string priceStr = ShowSimpleInput("Новий тариф", "Ціна (грн):", "0");
                    if (!string.IsNullOrEmpty(priceStr) && decimal.TryParse(priceStr, out decimal price) && price >= 0)
                    {
                        tariffs.Add(new FixedTariff(name, price));

                        // Оновлюємо ComboBox
                        if (tariffComboBox != null)
                        {
                            tariffComboBox.Items.Add($"{name} ({price} грн/міс)");
                        }

                        MessageBox.Show("Тариф створено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Введіть коректну ціну!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення тарифу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ShowSimpleInput(string title, string prompt, string defaultValue)
        {
            try
            {
                Form inputForm = new Form()
                {
                    Width = 400,
                    Height = 180,
                    Text = title,
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(45, 45, 85),
                    ForeColor = Color.White,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                Label label = new Label()
                {
                    Left = 20,
                    Top = 20,
                    Width = 350,
                    Text = prompt,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };

                TextBox textBox = new TextBox()
                {
                    Left = 20,
                    Top = 50,
                    Width = 350,
                    Text = defaultValue ?? "",
                    BackColor = Color.FromArgb(65, 65, 105),
                    ForeColor = Color.White
                };

                Button okButton = new Button()
                {
                    Text = "OK",
                    Left = 200,
                    Width = 80,
                    Top = 90,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(0, 150, 100),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                Button cancelButton = new Button()
                {
                    Text = "Скасувати",
                    Left = 290,
                    Width = 80,
                    Top = 90,
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(150, 50, 50),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                okButton.Click += (s, e) => { inputForm.Close(); };
                inputForm.Controls.Add(label);
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(okButton);
                inputForm.Controls.Add(cancelButton);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
            catch
            {
                return "";
            }
        }    
       
    }

    // ДІАЛОГОВІ ФОРМИ 
    public partial class AddClientDialog : Form
    {
        public Client CreatedClient { get; private set; }

        public AddClientDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Додати клієнта";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 85);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblName = new Label { Text = "Ім'я:", Location = new Point(20, 30), Size = new Size(100, 20), ForeColor = Color.White };
            TextBox txtName = new TextBox { Location = new Point(130, 30), Size = new Size(200, 25), BackColor = Color.FromArgb(65, 65, 105), ForeColor = Color.White };

            Label lblEmail = new Label { Text = "Email:", Location = new Point(20, 70), Size = new Size(100, 20), ForeColor = Color.White };
            TextBox txtEmail = new TextBox { Location = new Point(130, 70), Size = new Size(200, 25), BackColor = Color.FromArgb(65, 65, 105), ForeColor = Color.White };

            Label lblType = new Label { Text = "Тип клієнта:", Location = new Point(20, 110), Size = new Size(100, 20), ForeColor = Color.White };
            ComboBox cmbType = new ComboBox
            {
                Location = new Point(130, 110),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };
            cmbType.Items.AddRange(new[] { "Домашній", "Бізнес", "VIP" });
            cmbType.SelectedIndex = 0;

            Button btnOK = new Button
            {
                Text = "Створити",
                Location = new Point(130, 160),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 150, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(240, 160),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOK.Click += (s, e) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(txtName.Text?.Trim()) || string.IsNullOrEmpty(txtEmail.Text?.Trim()))
                    {
                        MessageBox.Show("Заповніть всі поля!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.DialogResult = DialogResult.None;
                        return;
                    }

                    switch (cmbType.SelectedIndex)
                    {
                        case 0: CreatedClient = new HomeUser(txtName.Text.Trim(), txtEmail.Text.Trim()); break;
                        case 1: CreatedClient = new BusinessUser(txtName.Text.Trim(), txtEmail.Text.Trim()); break;
                        case 2: CreatedClient = new VIPUser(txtName.Text.Trim(), txtEmail.Text.Trim()); break;
                        default: CreatedClient = new HomeUser(txtName.Text.Trim(), txtEmail.Text.Trim()); break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка створення клієнта: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblEmail, txtEmail, lblType, cmbType, btnOK, btnCancel });
        }
    }

    public partial class AddServiceDialog : Form
    {
        public Service CreatedService { get; private set; }

        public AddServiceDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Додати послугу";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 85);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblType = new Label { Text = "Тип послуги:", Location = new Point(20, 30), Size = new Size(100, 20), ForeColor = Color.White };
            ComboBox cmbType = new ComboBox
            {
                Location = new Point(130, 30),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };
            cmbType.Items.AddRange(new[] { "Інтернет", "Кабельне ТБ", "Телефонія" });
            cmbType.SelectedIndex = 0;

            Label lblValue = new Label { Text = "Швидкість (Мбіт/с):", Location = new Point(20, 70), Size = new Size(100, 20), ForeColor = Color.White };
            NumericUpDown numValue = new NumericUpDown
            {
                Location = new Point(130, 70),
                Size = new Size(200, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 50,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };

            cmbType.SelectedIndexChanged += (s, e) =>
            {
                switch (cmbType.SelectedIndex)
                {
                    case 0:
                        lblValue.Text = "Швидкість (Мбіт/с):";
                        numValue.Maximum = 1000;
                        numValue.Value = 50;
                        break;
                    case 1:
                        lblValue.Text = "Кількість каналів:";
                        numValue.Maximum = 500;
                        numValue.Value = 100;
                        break;
                    case 2:
                        lblValue.Text = "Хвилин на місяць:";
                        numValue.Maximum = 10000;
                        numValue.Value = 500;
                        break;
                }
            };

            Button btnOK = new Button
            {
                Text = "Додати",
                Location = new Point(130, 120),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(150, 0, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(240, 120),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOK.Click += (s, e) =>
            {
                try
                {
                    int value = (int)numValue.Value;
                    switch (cmbType.SelectedIndex)
                    {
                        case 0: CreatedService = new InternetService(value); break;
                        case 1: CreatedService = new TVService(value); break;
                        case 2: CreatedService = new PhoneService(value); break;
                        default: CreatedService = new InternetService(value); break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка створення послуги: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            };

            this.Controls.AddRange(new Control[] { lblType, cmbType, lblValue, numValue, btnOK, btnCancel });
        }
    }

    public partial class PaymentDialog : Form
    {
        public PaymentRecord Payment { get; private set; }

        public PaymentDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Новий платіж";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 85);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblAmount = new Label { Text = "Сума (грн):", Location = new Point(20, 30), Size = new Size(100, 20), ForeColor = Color.White };
            NumericUpDown numAmount = new NumericUpDown
            {
                Location = new Point(130, 30),
                Size = new Size(200, 25),
                Minimum = 0,
                Maximum = 100000,
                Value = 500,
                DecimalPlaces = 2,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };

            Label lblDesc = new Label { Text = "Опис:", Location = new Point(20, 70), Size = new Size(100, 20), ForeColor = Color.White };
            TextBox txtDesc = new TextBox
            {
                Location = new Point(130, 70),
                Size = new Size(200, 80),
                Multiline = true,
                Text = "Оплата послуг інтернет-провайдера",
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };

            Button btnOK = new Button
            {
                Text = "Провести",
                Location = new Point(130, 180),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 200, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(240, 180),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOK.Click += (s, e) =>
            {
                try
                {
                    Payment = new PaymentRecord
                    {
                        Amount = numAmount.Value,
                        Date = DateTime.Now,
                        Description = txtDesc.Text?.Trim() ?? "Платіж"
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка створення платежу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            };

            this.Controls.AddRange(new Control[] { lblAmount, numAmount, lblDesc, txtDesc, btnOK, btnCancel });
        }
    }

    public partial class MassPaymentDialog : Form
    {
        private List<Client> clients;

        public MassPaymentDialog(List<Client> clients)
        {
            this.clients = clients ?? new List<Client>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Масові платежі";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 85);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblAmount = new Label { Text = "Сума для всіх (грн):", Location = new Point(20, 30), Size = new Size(150, 20), ForeColor = Color.White };
            NumericUpDown numAmount = new NumericUpDown
            {
                Location = new Point(180, 30),
                Size = new Size(150, 25),
                Minimum = 0,
                Maximum = 100000,
                Value = 300,
                DecimalPlaces = 2,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };

            CheckedListBox chkClients = new CheckedListBox
            {
                Location = new Point(20, 70),
                Size = new Size(440, 150),
                BackColor = Color.FromArgb(25, 25, 45),
                ForeColor = Color.White
            };

            if (clients != null)
            {
                foreach (var client in clients)
                {
                    if (client != null)
                        chkClients.Items.Add($"{client.GetName() ?? "Без імені"} ({client.GetEmail() ?? "Без email"})", true);
                }
            }

            Button btnSelectAll = new Button
            {
                Text = "Обрати всіх",
                Location = new Point(20, 240),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSelectAll.Click += (s, e) =>
            {
                for (int i = 0; i < chkClients.Items.Count; i++)
                    chkClients.SetItemChecked(i, true);
            };

            Button btnSelectNone = new Button
            {
                Text = "Зняти всі",
                Location = new Point(130, 240),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(200, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSelectNone.Click += (s, e) =>
            {
                for (int i = 0; i < chkClients.Items.Count; i++)
                    chkClients.SetItemChecked(i, false);
            };

            Button btnOK = new Button
            {
                Text = "Провести платежі",
                Location = new Point(250, 240),
                Size = new Size(120, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(0, 200, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(380, 240),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOK.Click += (s, e) =>
            {
                try
                {
                    int processed = 0;
                    for (int i = 0; i < chkClients.CheckedItems.Count; i++)
                    {
                        if (i < chkClients.CheckedIndices.Count)
                        {
                            int clientIndex = chkClients.CheckedIndices[i];
                            if (clientIndex >= 0 && clientIndex < clients.Count && clients[clientIndex] != null)
                            {
                                var payment = new PaymentRecord
                                {
                                    Amount = numAmount.Value,
                                    Date = DateTime.Now,
                                    Description = "Масовий платіж"
                                };
                                clients[clientIndex].AddPayment(payment);
                                processed++;
                            }
                        }
                    }
                    MessageBox.Show($"Оброблено {processed} платежів!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка масових платежів: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            };

            this.Controls.AddRange(new Control[] { lblAmount, numAmount, chkClients, btnSelectAll, btnSelectNone, btnOK, btnCancel });
        }
    }

    public partial class TariffCalculatorForm : Form
    {
        private List<ITariff> tariffs;

        public TariffCalculatorForm(List<ITariff> tariffs)
        {
            this.tariffs = tariffs ?? new List<ITariff>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Калькулятор тарифів";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 85);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTariff = new Label { Text = "Тариф:", Location = new Point(20, 30), Size = new Size(100, 20), ForeColor = Color.White };
            ComboBox cmbTariff = new ComboBox
            {
                Location = new Point(130, 30),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };

            if (tariffs != null)
            {
                foreach (var tariff in tariffs)
                {
                    if (tariff != null)
                        cmbTariff.Items.Add(tariff.GetTariffName());
                }
                if (cmbTariff.Items.Count > 0)
                    cmbTariff.SelectedIndex = 0;
            }

            Label lblValue = new Label { Text = "Кількість:", Location = new Point(20, 70), Size = new Size(100, 20), ForeColor = Color.White };
            NumericUpDown numValue = new NumericUpDown
            {
                Location = new Point(130, 70),
                Size = new Size(150, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 1,
                BackColor = Color.FromArgb(65, 65, 105),
                ForeColor = Color.White
            };

            Label lblResult = new Label
            {
                Text = "Вартість: 0 грн",
                Location = new Point(20, 110),
                Size = new Size(300, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 255, 255)
            };

            Button btnCalculate = new Button
            {
                Text = "Розрахувати",
                Location = new Point(130, 160),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 150, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            EventHandler calculate = (s, e) =>
            {
                try
                {
                    if (cmbTariff.SelectedIndex >= 0 && tariffs != null && tariffs.Count > cmbTariff.SelectedIndex)
                    {
                        var selectedTariff = tariffs[cmbTariff.SelectedIndex];
                        if (selectedTariff != null)
                        {
                            decimal price = selectedTariff.CalculatePrice((int)numValue.Value);
                            lblResult.Text = $"Вартість: {price:F2} грн";
                        }
                    }
                }
                catch
                {
                    lblResult.Text = "Вартість: 0 грн";
                }
            };

            btnCalculate.Click += calculate;
            cmbTariff.SelectedIndexChanged += calculate;
            numValue.ValueChanged += calculate;

            Button btnClose = new Button
            {
                Text = "Закрити",
                Location = new Point(260, 160),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Початковий розрахунок
            calculate(null, null);

            this.Controls.AddRange(new Control[] { lblTariff, cmbTariff, lblValue, numValue, lblResult, btnCalculate, btnClose });
        }
    }

    
}
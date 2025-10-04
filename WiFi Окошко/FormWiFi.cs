// Copyright (c) 2025 Otto
// Лицензия: MIT (см. LICENSE)

using CredentialManagement;
using QRCoder;
using Renci.SshNet;
using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFi_Окошко
{
    internal partial class FormWiFi : Form
    {
        // Имя записи в диспетчере учётных данных Windows (Credential Manager) для логина с портом (формат "User:222") и пароля
        const string CredentialTarget = "WiFi_Window";

        // Таймер автозавершения (обновляет заголовок окна)
        private Timer autoCloseTimer;

        // Интервал автозавершения в миллисекундах (по умолчанию 1 минута)
        private const int AutoCloseIntervalMs = 60_000;

        // Оставшееся время в секундах (используется для отображения обратного отсчёта)
        private int remainingSeconds;

        // Базовый заголовок формы (значение "this.Text" из дизайнера)
        private readonly string baseTitle;

        // Минимальная и максимальная ширина для SSID/Passwd
        private const int MinLabelWidth = 150;
        private const int MaxLabelWidth = 300;

        // Максимальная ширина формы
        private const int MaxFormWidth = 517;

        // Отступ между Label и PictureBox
        private const int Gap = 10;

        internal FormWiFi()
        {
            InitializeComponent();
            this.Load += FormWiFi_Load;

            // Сохраняем первоначальный заголовк
            baseTitle = string.IsNullOrWhiteSpace(this.Text) ? "WiFi Окошко" : this.Text.Trim();
        }

        // Главный обработчик загрузки: ищет шлюз, загружает креды, извлекает порт, запускает подключение и обновляет UI
        private async void FormWiFi_Load(object sender, EventArgs e)
        {
            // Показываем начальное состояние только в поле SSID
            SetLabelText(SSID, "Ждём...");

            // Запускаем таймер автозавершения (начинает отсчёт с запуска формы)
            StartAutoCloseTimer();

            var gw = GetDefaultGatewayIPv4();
            if (gw == null)
            {
                SetLabelText(SSID, "Шлюз не найден");   // Показываем ошибку в SSID
                SetControlFontBold(SSID, false);        // Делаем обычный шрифт
                SetLabelText(Passwd, string.Empty);     // Очищаем пароль
                SetControlFontBold(Passwd, false);      // Делаем обычный шрифт
                return;
            }

            // Загружаем учётные данные из диспетчера учётных данных Windows
            if (!TryLoadCredentials(CredentialTarget, out var sshUser, out var sshPass))
            {
                SetLabelText(SSID, "Учётные данные не найдены");    // Показываем ошибку в SSID
                SetControlFontBold(SSID, false);                    // Делаем обычный шрифт
                SetLabelText(Passwd, string.Empty);                 // Очищаем пароль
                SetControlFontBold(Passwd, false);                  // Делаем обычный шрифт
                return;
            }

            // Порт берётся из имени пользователя (формат "User:222")
            int sshPort = -1;

            // Извлекаем порт из имени пользователя
            if (!string.IsNullOrEmpty(sshUser))
            {
                var split = sshUser.Split([':'], StringSplitOptions.RemoveEmptyEntries);
                if (split.Length >= 2)
                {
                    var lastPart = split[split.Length - 1];
                    if (int.TryParse(lastPart, out var p2) && p2 > 0 && p2 <= 65535)
                    {
                        sshPort = p2;

                        // Восстанавливаем имя пользователя без порта
                        char sep = ':';
                        sshUser = string.Join(sep.ToString(), split, 0, split.Length - 1);
                    }
                }
            }

            // Порт не найден
            if (sshPort == -1)
            {
                SetLabelText(SSID, "Порт не задан");    // Показываем ошибку в SSID
                SetControlFontBold(SSID, false);        // Делаем обычный шрифт
                SetLabelText(Passwd, string.Empty);     // Очищаем пароль
                SetControlFontBold(Passwd, false);      // Делаем обычный шрифт
                return;
            }

            try
            {
                // Выполняем подключение асинхронно (в Task.Run, чтобы не блокировать UI)
                var tuple = await Task.Run(() => ConnectAndFetchFromMikrotik(gw.ToString(), sshPort, sshUser, sshPass));
                var ssid = tuple.ssid;
                var passwd = tuple.passwd;
                var profileBlock = tuple.profileBlock;

                if (!string.IsNullOrEmpty(ssid))
                {
                    // Успешно получили SSID — показываем и ставим жирный шрифт
                    SetLabelText(SSID, ssid);
                    SetControlFontBold(SSID, true);
                }
                else
                {
                    // Нет SSID — показываем сообщение об ошибке в SSID и делаем обычный шрифт
                    SetLabelText(SSID, "SSID не найден");
                    SetControlFontBold(SSID, false);
                }

                if (!string.IsNullOrEmpty(passwd))
                {
                    // Успешно получили пароль — показываем и ставим жирный шрифт
                    SetLabelText(Passwd, passwd);
                    SetControlFontBold(Passwd, true);

                    ShowQrCode(ssid, passwd, profileBlock); // Показываем QR в "PictureBoxQr"

                    // Подгоняем размеры и ширину формы
                    AdjustControlsWidthAndForm();

                }
                else
                {
                    // Пароля нет — очищаем поле и делаем обычный шрифт
                    SetLabelText(Passwd, string.Empty);
                    SetControlFontBold(Passwd, false);
                }
            }
            catch (Exception ex)
            {
                // При исключении: показываем ошибку в SSID (не жирным), пароль очищаем
                SetLabelText(SSID, "Ошибка: " + ex.Message);
                SetControlFontBold(SSID, false);

                SetLabelText(Passwd, string.Empty);
                SetControlFontBold(Passwd, false);
            }
        }

        // Вспомогательный метод: измеряет ширину текста внутри RichTextBox + запас (pad)
        private int MeasureTextWidthWithPadding(RichTextBox rtb, int paddingPx)
        {
            if (rtb == null) return MinLabelWidth;
            using Graphics g = rtb.CreateGraphics();

            // MeasureString иногда добавляет небольшой хвост; Ceiling округляет в большую сторону
            SizeF size = g.MeasureString(rtb.Text ?? string.Empty, rtb.Font);
            return (int)Math.Ceiling(size.Width) + paddingPx;
        }

        // Обновляем ширину RichTextBox, позицию QR и ширину формы
        private void AdjustControlsWidthAndForm()
        {
            if (SSID == null || Passwd == null || PictureBoxQr == null) return;

            // Измеряем требуемую ширину для каждого поля (без применения к форме)
            int widthSsid = MeasureTextWidthWithPadding(SSID, 5);
            int widthPass = MeasureTextWidthWithPadding(Passwd, 5);

            // Ограничиваем по Min/Max
            widthSsid = Math.Min(MaxLabelWidth, Math.Max(MinLabelWidth, widthSsid));
            widthPass = Math.Min(MaxLabelWidth, Math.Max(MinLabelWidth, widthPass));

            // Устанавливаем ширины (не двигаем Left'ы)
            SSID.Width = widthSsid;
            Passwd.Width = widthPass;

            // Правый край каждого поля
            int rightSsid = SSID.Left + SSID.Width;
            int rightPass = Passwd.Left + Passwd.Width;

            // Берём максимальную правую границу (чтобы QR оказался правее наиболее широкего поля)
            int rightMost = Math.Max(rightSsid, rightPass);

            // Новая позиция QR — справа от самого правого поля + промежуток Gap
            int newQrLeft = rightMost + Gap;

            // Рассчитаем необходимую ширину формы (с запасом справа)
            int neededFormWidth = newQrLeft + PictureBoxQr.Width + 15;

            // Если нужно, расширяем форму, но не больше MaxFormWidth
            int newFormWidth = Math.Min(MaxFormWidth, Math.Max(this.Width, neededFormWidth));
            this.Width = newFormWidth;

            // Если форма очень узкая (например ограничено MaxFormWidth) — не даём QR уйти за правую границу формы
            int maxAllowedQrLeft = Math.Max(0, this.Width - PictureBoxQr.Width - 15);
            if (newQrLeft > maxAllowedQrLeft)
                newQrLeft = maxAllowedQrLeft;

            // Двигаем PictureBox
            PictureBoxQr.Left = newQrLeft;
        }

        // Метод подгоняет ширину RichTextBox по тексту и сдвигает QR, увеличивая форму по необходимости
        private void AdjustRichTextBoxWidthAndForm(RichTextBox rtb, PictureBox qr)
        {
            if (rtb == null || qr == null) return;

            // Если вызван не из UI-потока — перенаправим выполнение в UI-поток
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AdjustRichTextBoxWidthAndForm(rtb, qr)));
                return;
            }

            // Гарантируем, что PictureBox не "приклеен" к правому краю или докнут
            qr.Dock = DockStyle.None;
            qr.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            const int rightPadding = 15; // Отступ от правого края PictureBox до края формы
            this.SuspendLayout();

            try
            {
                using Graphics g = rtb.CreateGraphics();
                // Измеряем ширину текста с текущим шрифтом
                SizeF textSize = g.MeasureString(rtb.Text, rtb.Font);

                // Ширина (с запасом)
                int desiredWidth = (int)Math.Ceiling(textSize.Width) + 5;
                desiredWidth = Math.Max(MinLabelWidth, Math.Min(MaxLabelWidth, desiredWidth));

                // Максимально возможная ширина RichTextBox при текущем MaxFormWidth
                int maxAllowedWidth = MaxFormWidth - (qr.Width + Gap + rtb.Left + rightPadding);
                if (maxAllowedWidth < MinLabelWidth) maxAllowedWidth = MinLabelWidth;

                if (desiredWidth <= maxAllowedWidth)
                {
                    // Ширина помещается без превышения MaxFormWidth
                    rtb.Width = desiredWidth;

                    int neededFormWidth = rtb.Left + rtb.Width + Gap + qr.Width + rightPadding;

                    // Увеличиваем форму, если нужно (но не превышаем MaxFormWidth)
                    if (neededFormWidth > this.Width)
                        this.Width = Math.Min(MaxFormWidth, neededFormWidth);
                }
                else
                {
                    // Ширина НЕ помещается в рамках MaxFormWidth, попробуем расширить форму до нужного размера (если возможно)
                    int neededFormWidth = rtb.Left + desiredWidth + Gap + qr.Width + rightPadding;
                    if (neededFormWidth <= MaxFormWidth)
                    {
                        // Можем расширить форму и поставить desiredWidth
                        this.Width = Math.Max(this.Width, neededFormWidth);
                        rtb.Width = desiredWidth;
                    }
                    else
                    {
                        // Нельзя расширить до нужного — ограничиваем rtb так, чтобы QR оставался видимым
                        rtb.Width = Math.Max(MinLabelWidth, maxAllowedWidth);

                        // И ставим форму в максимум
                        this.Width = MaxFormWidth;
                    }
                }

                // После изменения ширины rtb — корректируем позицию QR
                qr.Left = rtb.Left + rtb.Width + Gap;

                // Коррекция: не позволяем qr уйти за правую границу client area
                int clientRight = this.ClientSize.Width;
                if (qr.Left + qr.Width + rightPadding > clientRight)
                {
                    int newLeft = Math.Max(rtb.Left + rtb.Width + Gap, clientRight - qr.Width - rightPadding);
                    qr.Left = newLeft;
                }

                // Принудительная перерисовка
                qr.Refresh();
                rtb.Refresh();
                this.Refresh();
            }
            finally
            {
                this.ResumeLayout();
            }
        }


        // Генерирует QR-код из SSID и пароля и отображает его в PictureBoxQr
        private void ShowQrCode(string ssid, string password, string chosenProfileBlock)
        {
            if (string.IsNullOrEmpty(ssid)) return;

            string wifiType;

            if (string.IsNullOrEmpty(password))
            {
                wifiType = "nopass";
            }
            else if (chosenProfileBlock != null)
            {
                bool hasWpa2 = Regex.IsMatch(chosenProfileBlock, @"\bwpa2-pre-shared-key=""[^""]+""", RegexOptions.IgnoreCase);
                bool hasWpa = Regex.IsMatch(chosenProfileBlock, @"\bwpa-pre-shared-key=""[^""]+""", RegexOptions.IgnoreCase);

                if (hasWpa2) wifiType = "WPA2";
                else if (hasWpa) wifiType = "WPA";
                else wifiType = "WPA"; // Пароль есть, но ключей явно нет — безопаснее WPA
            }
            else
            {
                wifiType = "WPA"; // Запасной вариант
            }

            // Формируем текст для QR-кода
            string qrText = $"WIFI:T:{wifiType};S:{ssid};P:{password};";

            using var qrGen = new QRCodeGenerator();
            var qrData = qrGen.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            using var qr = new QRCode(qrData);

            // Генерируем исходный QR, 1 модуль = 1 пиксель
            Bitmap qrBmp = qr.GetGraphic(1, Color.Black, Color.White, true);

            int targetWidth = PictureBoxQr.Width;
            int targetHeight = PictureBoxQr.Height;

            // Вычисляем масштаб, чтобы QR вписался в PictureBox
            float scaleX = (float)targetWidth / qrBmp.Width;
            float scaleY = (float)targetHeight / qrBmp.Height;
            float scale = Math.Min(scaleX, scaleY); // Cохраняем пропорции

            int newWidth = (int)(qrBmp.Width * scale);
            int newHeight = (int)(qrBmp.Height * scale);

            // Центрируем QR в PictureBox
            Bitmap finalBmp = new(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(finalBmp))
            {
                g.Clear(Color.White);
                int x = (targetWidth - newWidth) / 2;
                int y = (targetHeight - newHeight) / 2;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(qrBmp, x, y, newWidth, newHeight);
            }

            // Помещаем в контрол
            PictureBoxQr.Image = finalBmp;
        }

        // Попытка загрузить имя пользователя и пароль из указанного Target в Credential Manager (возвращает true, если успешно прочитаны)
        private static bool TryLoadCredentials(string target, out string username, out string password)
        {
            username = null;
            password = null;

            try
            {
                using var cred = new Credential { Target = target, Type = CredentialType.Generic };
                if (!cred.Load())
                    return false;

                username = cred.Username;
                password = cred.Password;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Запускает таймер с интервалом 1 сек для обновления заголовка и автозакрытия
        private void StartAutoCloseTimer()
        {
            // Если уже есть — остановим и удалим
            if (autoCloseTimer != null)
            {
                autoCloseTimer.Stop();
                autoCloseTimer.Tick -= AutoCloseTimer_Tick;
                autoCloseTimer.Dispose();
                autoCloseTimer = null;
            }

            remainingSeconds = AutoCloseIntervalMs / 1000;

            autoCloseTimer = new Timer
            {
                Interval = 1000 // Обновляем каждую секунду
            };
            autoCloseTimer.Tick += AutoCloseTimer_Tick;
            autoCloseTimer.Start();

            // Сразу обновим заголовок
            UpdateWindowTitle(remainingSeconds);
        }

        // Обновляет обратный отсчёт в заголовке и корректно завершает приложение при нуле
        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds = Math.Max(0, remainingSeconds - 1);

            // Обновляем заголовок
            UpdateWindowTitle(remainingSeconds);

            if (remainingSeconds <= 0)
            {
                // Остановим таймер и корректно завершим приложение
                if (autoCloseTimer != null)
                {
                    autoCloseTimer.Stop();
                    autoCloseTimer.Tick -= AutoCloseTimer_Tick;
                    autoCloseTimer.Dispose();
                    autoCloseTimer = null;
                }

                Application.Exit();
            }
        }

        // Обновляет "this.Text" в формате: "{baseTitle} | Завершение через: NN сек."
        private void UpdateWindowTitle(int secondsLeft)
        {
            try
            {
                this.Text = $"{baseTitle} | (Завершение через: {secondsLeft} сек.)";
            }
            catch
            {
                // В редком случае защиты от потоков — игнорируем
            }
        }

        // Универсальный установщик текста: если контрол RichTextBox - делаем цветной рендер, иначе обычный текст
        private void SetLabelText(Control ctrl, string text)
        {
            if (ctrl == null) return;

            // Если вызов из другого потока — используем Invoke
            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(new Action(() => SetLabelText(ctrl, text)));
                return;
            }

            if (ctrl is RichTextBox rtb)
            {
                // Для RichTextBox используем посимвольный рендер
                RenderColoredText(rtb, text ?? string.Empty);
            }
            else if (ctrl is Label lbl)
            {
                lbl.Text = text ?? string.Empty;
            }
            else
            {
                ctrl.Text = text ?? string.Empty;
            }
        }

        // Устанавливает жирность шрифта (true = Bold, false = Regular)
        private void SetControlFontBold(Control ctrl, bool bold)
        {
            if (ctrl == null) return;

            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(new Action(() => SetControlFontBold(ctrl, bold)));
                return;
            }

            var currentFont = ctrl.Font ?? SystemFonts.DefaultFont;
            FontStyle style = bold ? FontStyle.Bold : FontStyle.Regular;
            // Сохраняем размер и семейство, заменяем только стиль
            ctrl.Font = new Font(currentFont.FontFamily, currentFont.Size, style);
        }

        // Рендерим текст по-символьно: буквы - чёрные, цифры - синие, спецсимволы - розовые
        private void RenderColoredText(RichTextBox rtb, string text)
        {
            if (rtb == null) return;

            rtb.SuspendLayout();
            rtb.ReadOnly = false;
            rtb.Clear();

            // Настройка стиля рендера
            var colorLetter = Color.Black;
            var colorDigit = Color.Blue;
            var colorSpecial = Color.DeepPink;

            foreach (var ch in text)
            {
                if (char.IsLetter(ch))
                {
                    rtb.SelectionColor = colorLetter;
                }
                else if (char.IsDigit(ch))
                {
                    rtb.SelectionColor = colorDigit;
                }
                else
                {
                    // Пробелы оставим нейтральными (черными)
                    if (char.IsWhiteSpace(ch))
                        rtb.SelectionColor = colorLetter;
                    else
                        rtb.SelectionColor = colorSpecial;
                }

                rtb.AppendText(ch.ToString());
            }

            // Сбрасываем выделение и ставим в начало
            rtb.SelectionStart = 0;
            rtb.SelectionLength = 0;
            rtb.ReadOnly = true;
            rtb.ResumeLayout();
        }

        // Логика подключения и парсинга (с добавлением декодирования escape-последовательностей)
        private (string ssid, string passwd, string profileBlock) ConnectAndFetchFromMikrotik(string host, int port, string user, string pass)
        {
            // Используем SshClient для подключения
            using var client = new SshClient(host, port, user, pass);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(8);
            client.Connect();
            if (!client.IsConnected)
                throw new Exception("Не удалось подключиться по SSH");

            try
            {
                // Получаем список интерфейсов и security-profiles
                var wirelessRaw = client.CreateCommand("/interface wireless print").Execute() ?? "";
                var profilesRaw = client.CreateCommand("/interface wireless security-profiles print").Execute() ?? "";

                // Удаляем визуальные отступы Mikrotik (если строки длинные)
                profilesRaw = NormalizeMikrotikWrappedLines(profilesRaw);

                var ifaceBlocks = SplitIntoIndexBlocks(wirelessRaw);
                
                string chosenIfBlock = null;
                foreach (var blk in ifaceBlocks)
                {
                    // Ищем интерфейс с 2.4 GHz
                    var bandM = Regex.Match(blk, @"\bband=(?<band>\S+)", RegexOptions.IgnoreCase);
                    if (bandM.Success && bandM.Groups["band"].Value.ToLower().Contains("2ghz"))
                    {
                        chosenIfBlock = blk;
                        break;
                    }
                    if (blk.IndexOf("2ghz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        blk.IndexOf("2.4", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        chosenIfBlock = blk;
                        break;
                    }
                }

                // Если не нашли 2.4 GHz — пробуем wlan или первый блок
                chosenIfBlock ??= ifaceBlocks.FirstOrDefault(b => Regex.IsMatch(b, @"\bname=""wlan", RegexOptions.IgnoreCase));

                if (chosenIfBlock == null && ifaceBlocks.Length > 0)
                    chosenIfBlock = ifaceBlocks[0];

                string ssid = null;
                string ifaceName = null;
                string securityProfile = null;

                if (!string.IsNullOrEmpty(chosenIfBlock))
                {
                    // Извлекаем имя интерфейса, SSID и security-profile
                    var nameM = Regex.Match(chosenIfBlock, @"\bname=""(?<name>[^""]+)""", RegexOptions.IgnoreCase);
                    if (nameM.Success) ifaceName = nameM.Groups["name"].Value.Trim();

                    var ssidM = Regex.Match(chosenIfBlock, @"\bssid=""(?<ssid>[^""]+)""", RegexOptions.IgnoreCase);
                    if (ssidM.Success) ssid = ssidM.Groups["ssid"].Value.Trim();

                    var profM = Regex.Match(chosenIfBlock, @"\bsecurity-profile=(?<prof>\S+)", RegexOptions.IgnoreCase);
                    if (profM.Success) securityProfile = TrimQuotes(profM.Groups["prof"].Value.Trim());
                }

                // Если профиль не найден — пробуем получить его напрямую по имени интерфейса
                if (string.IsNullOrEmpty(securityProfile) && !string.IsNullOrEmpty(ifaceName))
                {
                    try
                    {
                        var cmd = $":put [/interface wireless get [find name=\"{EscapeForMikrotik(ifaceName)}\"] security-profile]";
                        var res = client.CreateCommand(cmd).Execute();
                        if (!string.IsNullOrWhiteSpace(res))
                            securityProfile = res.Trim();
                    }
                    catch { }
                }

                // Если SSID не найден — пробуем напрямую по имени интерфейса
                if (string.IsNullOrEmpty(ssid) && !string.IsNullOrEmpty(ifaceName))
                {
                    try
                    {
                        var cmd = $":put [/interface wireless get [find name=\"{EscapeForMikrotik(ifaceName)}\"] ssid]";
                        var res = client.CreateCommand(cmd).Execute();
                        if (!string.IsNullOrWhiteSpace(res))
                            ssid = res.Trim();
                    }
                    catch { }
                }

                var profBlocks = SplitIntoIndexBlocks(profilesRaw);

                string chosenProfileBlock = null;

                // Пробуем найти блок security-profile по имени
                if (!string.IsNullOrEmpty(securityProfile))
                {
                    chosenProfileBlock = profBlocks.FirstOrDefault(b =>
                        Regex.IsMatch(b, $@"\bname=""{Regex.Escape(securityProfile)}""", RegexOptions.IgnoreCase));
                }

                // Если не нашли — ищем где явно указан ключ
                chosenProfileBlock ??= profBlocks.FirstOrDefault(b =>
                    Regex.IsMatch(b, @"\bwpa2-pre-shared-key=""(?<k>[^""]+)""", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(b, @"\bwpa-pre-shared-key=""(?<k>[^""]+)""", RegexOptions.IgnoreCase));

                // Если и так не нашли — берём первый активный профиль (*)
                chosenProfileBlock ??= profBlocks.FirstOrDefault(b => Regex.IsMatch(b, @"^\s*\d+\s+\*", RegexOptions.Multiline));

                // Если профиль известен, но блока нет — пробуем вычитать напрямую ключ
                if (chosenProfileBlock == null && !string.IsNullOrEmpty(securityProfile))
                {
                    try
                    {
                        var cmd = $":put [/interface wireless security-profiles get [find name=\"{EscapeForMikrotik(securityProfile)}\"] wpa2-pre-shared-key]";
                        var res = client.CreateCommand(cmd).Execute();
                        if (!string.IsNullOrWhiteSpace(res))
                        {
                            chosenProfileBlock = $"name=\"{securityProfile}\" wpa2-pre-shared-key=\"{res.Trim()}\"";
                        }
                    }
                    catch { }
                }

                string passwd = null;
                if (!string.IsNullOrEmpty(chosenProfileBlock))
                {
                    // Ищем один из доступных ключей WPA2/WPA (в приоритете WPA2)
                    var keyFields = new[] { "wpa2-pre-shared-key", "wpa-pre-shared-key"};
                    foreach (var k in keyFields)
                    {
                        var m = Regex.Match(chosenProfileBlock, $@"\b{k}=""(?<val>[^""]*)""", RegexOptions.IgnoreCase);
                        if (m.Success && !string.IsNullOrEmpty(m.Groups["val"].Value))
                        {
                            passwd = m.Groups["val"].Value;
                            break;
                        }
                    }
                }

                // Декодируем возможные hex-escape последовательности вида "\HH\HH\HH"
                if (!string.IsNullOrEmpty(ssid))
                    ssid = DecodeMikrotikEscapes(ssid);

                if (!string.IsNullOrEmpty(passwd))
                    passwd = DecodeMikrotikEscapes(passwd);

                client.Disconnect();
                return (ssid, passwd, chosenProfileBlock);
            }
            finally
            {
                // Гарантированно отключаем SSH сессию
                if (client.IsConnected)
                    client.Disconnect();
            }
        }

        // Убирает отступы Mikrotik при переносах длинных строк
        private static string NormalizeMikrotikWrappedLines(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // У Mikrotik переносы длинных строк начинаются с 20 пробелов
            return Regex.Replace(input, @"\r?\n\s{20,}", string.Empty);
        }


        // Декодирует подряд идущие escape-последовательности вида "\HH\HH\HH" в текст
        // Если последовательность успешно даёт кириллический текст при декодировании Windows-1251 — используется он, иначе пробует UTF-8 и другие fallback'ы.
        // Не затрагивает обычный ASCII/латинский текст.
        private string DecodeMikrotikEscapes(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Найдём подряд идущие группы "\HH\HH..."
            return Regex.Replace(input, @"(\\[0-9A-Fa-f]{2})+", match =>
            {
                var token = match.Value; // Например "\D2\E5\F1\F2" (Тест)
                var hexMatches = Regex.Matches(token, @"\\([0-9A-Fa-f]{2})");
                var bytes = new byte[hexMatches.Count];
                for (int i = 0; i < hexMatches.Count; i++)
                {
                    bytes[i] = Convert.ToByte(hexMatches[i].Groups[1].Value, 16);
                }

                // Попробуем разные кодировки в порядке приоритетов:
                // 1) Windows-1251 (часто Mikrotik/Win-encoded Cyrillic)
                // 2) UTF-8
                // 3) ISO-8859-1 (как безопасный fallback)
                // 4) System.Text.Encoding.Default
                string TryWith(Encoding enc)
                {
                    try
                    {
                        return enc.GetString(bytes);
                    }
                    catch
                    {
                        return null;
                    }
                }

                var candidates = new (Encoding enc, string name)[]
                {
                    (Encoding.GetEncoding(1251), "cp1251"),
                    (Encoding.UTF8, "utf8"),
                    (Encoding.GetEncoding("ISO-8859-1"), "iso-8859-1"),
                    (Encoding.Default, "default")
                };

                // Сначала ищем декодирование, которое даёт кириллицу
                foreach (var (enc, name) in candidates)
                {
                    var s = TryWith(enc);
                    if (!string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"\p{IsCyrillic}"))
                        return s;
                }

                // Если кириллицы не оказалось, попробуем UTF-8 без символа замены
                var utf8 = TryWith(Encoding.UTF8);
                if (!string.IsNullOrEmpty(utf8) && !utf8.Contains("\uFFFD"))
                    return utf8;

                // Иначе пробуем CP1251
                var cp1251 = TryWith(Encoding.GetEncoding(1251));
                if (!string.IsNullOrEmpty(cp1251))
                    return cp1251;

                // В крайнем случае — вернём исходный token (чтобы не терять данные)
                return token;
            });
        }

        // Разбивает сырой вывод Mikrotik на индексированные блоки для удобного парсинга
        private string[] SplitIntoIndexBlocks(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return [];

            // Поиск начала блоков: строки начинающиеся с "  <число>"
            var matches = Regex.Matches(raw, @"(?m)^\s*\d+\b");
            if (matches.Count == 0)
            {
                return [raw];
            }

            var blocks = new System.Collections.Generic.List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                int start = matches[i].Index;
                int end = (i + 1 < matches.Count) ? matches[i + 1].Index : raw.Length;

                // Вырезаем участок текста от текущего индекса до следующего
                var blk = raw.Substring(start, end - start);
                blocks.Add(blk.Trim());
            }
            return [.. blocks];
        }

        // Убирает окружающие двойные кавычки из строки (если они есть)
        private string TrimQuotes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.StartsWith("\"") && s.EndsWith("\"") && s.Length >= 2)
                return s.Substring(1, s.Length - 2);
            return s;
        }

        // Экранирует символы (в частности двойные кавычки) для безопасной вставки в команды Mikrotik
        private string EscapeForMikrotik(string s)
        {
            if (s == null) return "";
            return s.Replace("\"", "\\\"");
        }

        // Находит IPv4-адрес шлюза по умолчанию среди активных сетевых интерфейсов для подключения к Mikrotik'у
        private IPAddress GetDefaultGatewayIPv4()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProps = ni.GetIPProperties();
                if (ipProps == null) continue;

                foreach (var ga in ipProps.GatewayAddresses)
                {
                    if (ga?.Address == null) continue;
                    if (ga.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        // Отбрасываем 127.0.0.1 и 0.0.0.0, берём реальный адрес шлюза
                        if (!IPAddress.IsLoopback(ga.Address) && !ga.Address.Equals(IPAddress.Parse("0.0.0.0")))
                        return ga.Address;
                    }
                }
            }
            return null;
        }

        // Ссылка на страницу автора
        private void LinkLabelAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://gitflic.ru/project/otto/wifi_window");
        }
    }
}

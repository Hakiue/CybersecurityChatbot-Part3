using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot.ChatbotCore
{
    /// <summary>
    /// Contains all predefined responses and the logic for matching user input
    /// to relevant cybersecurity topics.
    /// Part 2 requires dynamic responses, keyword recognition, lists/dictionaries,
    /// random answers, memory support and conversation-flow support.
    /// </summary>
    public class ResponseEngine
    {
        private readonly Random _random = new Random();

        // Topic aliases make the bot understand different ways users ask the same thing.
        private readonly Dictionary<string, string[]> _topicAliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] = new[] { "password", "passphrase", "login code", "credentials" },
            ["phishing"] = new[] { "phishing", "fake email", "email scam", "suspicious email" },
            ["safe browsing"] = new[] { "safe browsing", "browsing", "browser", "unsafe website", "https", "padlock" },
            ["two-factor authentication"] = new[] { "2fa", "two-factor", "two factor", "mfa", "multi-factor", "authentication", "otp" },
            ["cia triad"] = new[] { "cia triad", "cia", "confidentiality", "integrity", "availability" },
            ["encryption"] = new[] { "encryption", "encrypt", "encrypted", "cryptography" },
            ["malware"] = new[] { "malware", "virus", "trojan", "spyware", "keylogger", "malicious software" },
            ["ransomware"] = new[] { "ransomware", "encrypted files", "file ransom" },
            ["scams"] = new[] { "scam", "scams", "fraud", "fake offer", "fake payment", "online scam" },
            ["social engineering"] = new[] { "social engineering", "pretexting", "impersonation", "manipulate", "baiting" },
            ["smishing and vishing"] = new[] { "smishing", "vishing", "sms scam", "phone scam", "voice phishing" },
            ["wifi security"] = new[] { "wifi", "wi-fi", "public wifi", "router", "wireless" },
            ["vpn"] = new[] { "vpn", "virtual private network" },
            ["firewall"] = new[] { "firewall", "network firewall" },
            ["privacy"] = new[] { "privacy", "personal information", "personal data", "app permissions", "digital footprint" },
            ["popia"] = new[] { "popia", "protection of personal information act", "data privacy law" },
            ["data breach"] = new[] { "data breach", "breach", "leaked data", "data leak" },
            ["identity theft"] = new[] { "identity theft", "stolen identity", "id theft", "impersonate me" },
            ["sim swap"] = new[] { "sim swap", "sim-swap", "sim fraud", "stolen sim" },
            ["backup"] = new[] { "backup", "backups", "recovery", "restore files", "3-2-1" },
            ["cyber attack"] = new[] { "cyber attack", "cyberattack", "attack", "threat" },
            ["hacker"] = new[] { "hacker", "ethical hacking", "white hat", "black hat", "penetration test" },
            ["zero day"] = new[] { "zero day", "zero-day", "zero day attack", "zero-day attack", "zero day attacks", "zero-day attacks", "unpatched vulnerability" },
            ["sql injection"] = new[] { "sql injection", "sqli", "database attack" },
            ["man in the middle"] = new[] { "man in the middle", "mitm", "intercept" },
            ["patches"] = new[] { "patch", "patches", "patching", "security patch", "security patches", "updates", "software update", "update software" },
            ["antivirus"] = new[] { "antivirus", "anti-virus", "anti malware", "endpoint protection" },
            ["cyberbullying"] = new[] { "cyberbullying", "online bullying", "harassment" },
            ["reporting"] = new[] { "report", "where do i report", "report cybercrime", "csirt", "saps" },
        };

        // Rich explanations for each topic. These are longer so the bot can explain properly.
        private readonly Dictionary<string, string> _topicResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] =
                "🔑 Password safety protects your accounts from unauthorised access. A good password should be long, unique, and hard to guess.\n" +
                "   • Use at least 12–16 characters, or use a passphrase like three or four unrelated words.\n" +
                "   • Never reuse the same password across email, banking, school, shopping, and social media accounts.\n" +
                "   • Avoid birthdays, names, favourite teams, ID numbers, or common words like password123.\n" +
                "   • Use a password manager to store unique passwords safely.\n" +
                "   • Enable two-factor authentication so a stolen password alone is not enough to break in.",

            ["phishing"] =
                "🎣 Phishing is when attackers pretend to be a trusted person or organisation to trick you into revealing information.\n" +
                "   • They often copy banks, delivery companies, schools, SARS, streaming services, or employers.\n" +
                "   • Warning signs include urgent threats, spelling mistakes, strange sender addresses, unexpected attachments, and links that do not match the real website.\n" +
                "   • Do not click the link inside a suspicious message. Open the official website yourself or contact the organisation using official details.\n" +
                "   • Never share passwords, card PINs, or one-time PINs. A legitimate organisation should not ask for them in a message.",

            ["safe browsing"] =
                "🌐 Safe browsing means using the internet in a way that reduces your exposure to scams, malware, and fake websites.\n" +
                "   • Check for HTTPS and the correct domain before entering passwords or payment details.\n" +
                "   • Be careful with pop-ups, fake download buttons, and websites offering pirated software or free prizes.\n" +
                "   • Keep your browser updated because updates fix security weaknesses.\n" +
                "   • Download apps and files only from official stores or trusted publishers.\n" +
                "   • Log out on shared computers and avoid saving passwords on devices you do not own.",

            ["two-factor authentication"] =
                "🔐 Two-factor authentication, also called 2FA or MFA, adds another security step after your password.\n" +
                "   • It uses something you know, like a password, plus something you have, like an authenticator app code.\n" +
                "   • This means criminals still struggle to log in even if they steal your password.\n" +
                "   • Use an authenticator app where possible because it is safer than SMS.\n" +
                "   • Save your recovery codes somewhere safe so you do not get locked out.\n" +
                "   • Turn on 2FA first for email, banking, social media, and cloud storage accounts.",

            ["cia triad"] =
                "🏛️ The CIA Triad is the foundation of cybersecurity. It helps security teams decide whether information is properly protected.\n" +
                "   🔒 Confidentiality — only authorised people should access information. Example: encrypting customer records and using login permissions.\n" +
                "   ✅ Integrity — information must stay accurate, complete, and unchanged unless authorised. Example: preventing someone from secretly changing marks, payments, or bank balances.\n" +
                "   ⚡ Availability — systems and data must be accessible when authorised users need them. Example: keeping online banking available during high traffic or after a server failure.\n" +
                "   🛡 A strong security decision protects all three. For example, ransomware damages availability, phishing can damage confidentiality, and unauthorised editing damages integrity.",

            ["encryption"] =
                "🔒 Encryption converts readable information into unreadable code so only someone with the correct key can read it.\n" +
                "   • It protects data in transit, such as information sent through HTTPS websites.\n" +
                "   • It protects data at rest, such as files stored on a laptop, phone, or cloud drive.\n" +
                "   • Encryption is important for banking, messaging apps, Wi-Fi security, backups, and business data.\n" +
                "   • Even if a device is stolen, encrypted data is much harder for criminals to use.\n" +
                "   • Encryption does not stop all attacks, so it must be combined with passwords, updates, and access control.",

            ["malware"] =
                "🦠 Malware is malicious software designed to damage devices, steal information, spy on users, or give attackers control.\n" +
                "   • Viruses attach to files and spread when opened.\n" +
                "   • Trojans pretend to be useful programs but secretly do harm.\n" +
                "   • Spyware monitors activity and can steal passwords or banking details.\n" +
                "   • Keyloggers record what you type, including usernames and passwords.\n" +
                "   • Protect yourself with updates, antivirus, safe downloads, backups, and caution with email attachments.",

            ["ransomware"] =
                "💣 Ransomware is malware that locks or encrypts your files and demands payment to restore access.\n" +
                "   • It often spreads through phishing emails, unsafe downloads, weak remote access, or unpatched systems.\n" +
                "   • Paying the ransom does not guarantee that files will be restored.\n" +
                "   • The best defence is regular backups, especially backups that are not always connected to your computer.\n" +
                "   • Keep software updated, avoid suspicious attachments, and use strong passwords with 2FA.\n" +
                "   • Businesses should also train staff and test recovery plans.",

            ["scams"] =
                "⚠️ Online scams are attempts to trick you into sending money, sharing personal information, or giving access to an account.\n" +
                "   • Common scams include fake job offers, fake SARS refunds, romance scams, investment scams, fake proof of payment, and marketplace scams.\n" +
                "   • Scammers create urgency, fear, excitement, or pressure so you act without thinking.\n" +
                "   • Always verify using official contact details, not the phone number or link in the message.\n" +
                "   • Never share OTPs, passwords, card PINs, or remote access to your device.\n" +
                "   • If something feels too good to be true, slow down and check it carefully.",

            ["social engineering"] =
                "🕵️ Social engineering attacks people rather than systems. Attackers use trust, fear, urgency, or curiosity to make you do something unsafe.\n" +
                "   • Pretexting: the attacker invents a story, such as pretending to be IT support.\n" +
                "   • Baiting: the attacker offers something tempting, such as a free download or prize.\n" +
                "   • Impersonation: the attacker pretends to be a manager, bank employee, lecturer, or service provider.\n" +
                "   • Protect yourself by verifying identity, refusing pressure, and checking requests through official channels.\n" +
                "   • Remember: cybersecurity is not only technical; human decisions matter too.",

            ["smishing and vishing"] =
                "📱 Smishing and vishing are phishing attacks through SMS and phone calls.\n" +
                "   • Smishing uses text messages with fake links, fake delivery notices, fake bank alerts, or prize messages.\n" +
                "   • Vishing uses phone calls where scammers pretend to be banks, SARS, police, courier companies, or support agents.\n" +
                "   • Never give OTPs, passwords, PINs, or card details over the phone or through SMS links.\n" +
                "   • Hang up and call the organisation back using the official number from its website or your bank card.\n" +
                "   • Treat unexpected messages with caution, even if they include your name.",

            ["wifi security"] =
                "📶 Wi-Fi security protects your network and your data from attackers nearby or on the same network.\n" +
                "   • Public Wi-Fi can be risky because attackers may monitor traffic or create fake hotspots.\n" +
                "   • Avoid banking or entering sensitive information on public Wi-Fi unless you use a trusted VPN.\n" +
                "   • At home, use WPA2 or WPA3, a strong Wi-Fi password, and a changed router admin password.\n" +
                "   • Keep router firmware updated and disable WPS if you do not need it.\n" +
                "   • Use a guest network for visitors and smart home devices if possible.",

            ["vpn"] =
                "🛡 A VPN, or Virtual Private Network, creates an encrypted tunnel between your device and a VPN server.\n" +
                "   • It helps protect your traffic on public Wi-Fi from local attackers.\n" +
                "   • It can hide your activity from the Wi-Fi network owner, but it does not make you completely anonymous.\n" +
                "   • A VPN does not stop phishing, malware, weak passwords, or unsafe downloads.\n" +
                "   • Use a trusted VPN provider and avoid unknown free VPNs that may collect your data.\n" +
                "   • Think of a VPN as one layer of protection, not a complete security solution.",

            ["firewall"] =
                "🔥 A firewall controls network traffic entering or leaving a device or network.\n" +
                "   • It can block suspicious connections and reduce exposure to attacks.\n" +
                "   • Personal firewalls protect individual devices, while network firewalls protect whole networks.\n" +
                "   • Firewalls use rules to decide what traffic is allowed or blocked.\n" +
                "   • They work best with updates, antivirus, strong passwords, and safe browsing.\n" +
                "   • A firewall cannot protect you if you willingly give your password to a phishing website.",

            ["privacy"] =
                "🔒 Privacy is about controlling how your personal information is collected, shared, stored, and used.\n" +
                "   • Limit what you post publicly, especially your ID number, address, school/work details, travel plans, and banking information.\n" +
                "   • Review app permissions for camera, microphone, contacts, and location access.\n" +
                "   • Use privacy settings on social media and avoid accepting unknown friend requests.\n" +
                "   • Be careful with quizzes and apps that request unnecessary personal data.\n" +
                "   • Good privacy habits reduce identity theft, stalking, scams, and account recovery attacks.",

            ["popia"] =
                "🇿🇦 POPIA is South Africa's Protection of Personal Information Act. It protects people's personal information and regulates how organisations handle it.\n" +
                "   • Organisations should collect only necessary personal information and use it for a lawful purpose.\n" +
                "   • They must protect personal data against loss, misuse, unauthorised access, and unlawful sharing.\n" +
                "   • People have rights to know what information is held about them and to request correction or deletion in certain cases.\n" +
                "   • POPIA supports good cybersecurity because privacy and security work together.\n" +
                "   • For users, it means you should ask why your data is needed before sharing it.",

            ["data breach"] =
                "🚨 A data breach happens when confidential information is accessed, exposed, stolen, or shared without permission.\n" +
                "   • Breached data may include emails, passwords, ID numbers, phone numbers, addresses, or banking details.\n" +
                "   • After a breach, change passwords for affected accounts and any other account using the same password.\n" +
                "   • Watch for phishing because criminals often use leaked data to make scams more convincing.\n" +
                "   • Enable 2FA and monitor bank accounts or important services for suspicious activity.\n" +
                "   • Organisations should notify affected users and strengthen controls.",

            ["identity theft"] =
                "🪪 Identity theft happens when someone uses your personal information to pretend to be you.\n" +
                "   • Criminals may use stolen ID numbers, phone numbers, addresses, or account details to open accounts or commit fraud.\n" +
                "   • Protect your ID documents, avoid oversharing online, and be careful where you upload proof of identity.\n" +
                "   • Shred or securely delete documents containing personal information.\n" +
                "   • Use strong passwords and 2FA on email because email is often used to reset other accounts.\n" +
                "   • Act quickly if you notice unknown accounts, strange credit activity, or SIM swap warnings.",

            ["sim swap"] =
                "📲 SIM swap fraud happens when criminals move your cellphone number to a SIM card they control.\n" +
                "   • This lets them receive SMS OTPs and password reset codes.\n" +
                "   • Warning signs include suddenly losing signal, receiving SIM change messages, or being unable to receive calls and SMSs.\n" +
                "   • Contact your mobile provider and bank immediately if this happens.\n" +
                "   • Use authenticator apps instead of SMS OTP where possible.\n" +
                "   • Protect your personal information because criminals often need it to convince providers.",

            ["backup"] =
                "💾 Backups are copies of important data that help you recover after ransomware, theft, hardware failure, or accidental deletion.\n" +
                "   • Follow the 3-2-1 rule: keep 3 copies of important data, on 2 different storage types, with 1 copy offsite or in the cloud.\n" +
                "   • Back up photos, assignments, documents, business files, and passwords/recovery codes.\n" +
                "   • Test your backups occasionally to make sure you can restore them.\n" +
                "   • Do not keep all backups permanently connected because ransomware can encrypt connected drives.\n" +
                "   • Good backups turn a disaster into a recovery task.",

            ["cyber attack"] =
                "⚔️ A cyber attack is a deliberate attempt to steal, change, damage, or disrupt information systems.\n" +
                "   • Common attacks include phishing, malware, ransomware, DDoS, social engineering, and SQL injection.\n" +
                "   • Attackers may target money, personal information, business data, or system availability.\n" +
                "   • Defence requires layers: awareness, passwords, 2FA, updates, backups, access control, and monitoring.\n" +
                "   • Individuals and organisations are both targets, so everyone needs basic cybersecurity habits.\n" +
                "   • Early reporting helps limit damage.",

            ["hacker"] =
                "💻 A hacker is someone who uses technical skills to explore or manipulate systems. The word can mean different things depending on intent and permission.\n" +
                "   • White-hat hackers work legally to find and fix security weaknesses.\n" +
                "   • Black-hat hackers attack systems without permission for personal gain or harm.\n" +
                "   • Grey-hat hackers may find weaknesses without permission but do not always have malicious intent.\n" +
                "   • Ethical hacking, also called penetration testing, must be authorised.\n" +
                "   • Good cybersecurity uses ethical testing to improve defences before criminals exploit weaknesses.",

            ["zero day"] =
                "🕳️ A zero-day vulnerability is a software flaw that is unknown to the vendor or has no available patch yet.\n" +
                "   • It is dangerous because attackers can exploit it before users have a fix.\n" +
                "   • Once a patch is released, update quickly because criminals may then target unpatched systems.\n" +
                "   • Good defences include regular updates, antivirus, least privilege, backups, and monitoring suspicious behaviour.\n" +
                "   • Organisations reduce risk by limiting unnecessary software and access.\n" +
                "   • Users should avoid delaying security updates.",

            ["sql injection"] =
                "💉 SQL injection is a web application attack where malicious database commands are inserted into input fields.\n" +
                "   • Attackers may try to bypass login pages, view private data, change records, or delete information.\n" +
                "   • It happens when applications trust user input too much.\n" +
                "   • Developers prevent it using parameterised queries, input validation, least-privilege database accounts, and secure error handling.\n" +
                "   • For users, the main lesson is that poorly secured websites can expose your data.\n" +
                "   • For programmers, never build SQL by directly joining raw user input into a query.",

            ["man in the middle"] =
                "🕸️ A man-in-the-middle attack happens when an attacker secretly intercepts communication between two parties.\n" +
                "   • This can happen on unsafe Wi-Fi, fake hotspots, or compromised networks.\n" +
                "   • The attacker may read, alter, or steal information being sent.\n" +
                "   • HTTPS, VPNs, certificate warnings, and secure Wi-Fi reduce the risk.\n" +
                "   • Never ignore browser warnings about certificates on banking or login pages.\n" +
                "   • Avoid entering sensitive information on unknown public networks.",

            ["patches"] =
                "🧩 Patches and updates fix bugs and security weaknesses in software.\n" +
                "   • Cybercriminals often attack old vulnerabilities after patches are already available.\n" +
                "   • Update your operating system, browser, phone apps, antivirus, and router firmware.\n" +
                "   • Enable automatic updates where possible.\n" +
                "   • Restart devices when updates require it, because some fixes only apply after restart.\n" +
                "   • Updating is one of the simplest and strongest security habits.",

            ["antivirus"] =
                "🛡 Antivirus and anti-malware tools help detect, block, and remove malicious software.\n" +
                "   • They scan files, downloads, apps, and suspicious behaviour.\n" +
                "   • Keep antivirus updated so it recognises newer threats.\n" +
                "   • Antivirus is helpful, but it cannot replace safe behaviour.\n" +
                "   • Still avoid suspicious links, pirated software, unknown attachments, and fake support pop-ups.\n" +
                "   • Use it as part of a layered defence with backups, updates, and 2FA.",

            ["cyberbullying"] =
                "💬 Cyberbullying is harassment, threats, humiliation, or harmful behaviour using digital platforms.\n" +
                "   • Save evidence by taking screenshots and keeping messages.\n" +
                "   • Block and report the person on the platform.\n" +
                "   • Do not respond aggressively, because it can make the situation worse.\n" +
                "   • Speak to a trusted adult, lecturer, manager, or support service if it affects your safety or wellbeing.\n" +
                "   • Online safety includes emotional safety as well as technical security.",

            ["reporting"] =
                "📣 Reporting cybercrime helps reduce harm and can protect other people too.\n" +
                "   • Report suspicious messages to the platform, bank, employer, school, or service provider involved.\n" +
                "   • If money or identity documents are involved, contact your bank or provider immediately.\n" +
                "   • Keep evidence such as screenshots, email headers, links, phone numbers, and proof of payment.\n" +
                "   • Do not delete evidence before reporting.\n" +
                "   • In serious cases, report to the appropriate South African authorities or your organisation's IT/security team.",
        };

        // Random tips give varied answers for the Part 2 random-response requirement.
        private readonly Dictionary<string, List<string>> _randomResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] = new List<string>
            {
                "🎣 Phishing tip: hover over links before clicking. If the web address looks strange, shortened, misspelled, or unrelated to the organisation, do not open it.",
                "🎣 Phishing tip: urgency is a warning sign. Messages saying 'act now', 'your account will close', or 'payment failed' are designed to rush you.",
                "🎣 Phishing tip: never enter your password from a link in an email. Type the official website address yourself or use a trusted bookmark.",
                "🎣 Phishing tip: attachments can carry malware. Only open attachments you expected and trust.",
            },
            ["password"] = new List<string>
            {
                "🔑 Password tip: use a password manager so every account can have its own strong password.",
                "🔑 Password tip: a long passphrase is easier to remember and harder to crack than a short complicated password.",
                "🔑 Password tip: change reused passwords first, especially on email and banking accounts.",
                "🔑 Password tip: enable 2FA on important accounts so a leaked password is not enough to log in.",
            },
            ["safe browsing"] = new List<string>
            {
                "🌐 Safe browsing tip: check the domain carefully. 'bank.example.com' and 'bank-example.com' are not the same kind of address.",
                "🌐 Safe browsing tip: avoid pirated software websites because they are common sources of malware.",
                "🌐 Safe browsing tip: keep your browser updated and do not ignore security warnings.",
                "🌐 Safe browsing tip: be careful with fake download buttons and pop-up alerts claiming your device is infected.",
            },
            ["scams"] = new List<string>
            {
                "⚠️ Scam tip: never share OTPs. Scammers often need your OTP to complete fraud.",
                "⚠️ Scam tip: verify payment in your banking app before releasing goods to a buyer.",
                "⚠️ Scam tip: if a job, prize, or investment requires an upfront fee, treat it as suspicious.",
                "⚠️ Scam tip: call the organisation using official contact details, not the number in the suspicious message.",
            },
            ["privacy"] = new List<string>
            {
                "🔒 Privacy tip: review app permissions and remove access to your location, camera, microphone, or contacts when it is not needed.",
                "🔒 Privacy tip: avoid posting documents, tickets, addresses, or travel plans publicly.",
                "🔒 Privacy tip: use private social media settings and think carefully before accepting unknown people.",
                "🔒 Privacy tip: check what information recovery questions reveal about you. Do not use answers people can find online.",
            },
            ["sql injection"] = new List<string>
            {
                "💉 SQL injection tip: developers should use parameterised queries instead of joining raw user input into SQL commands.",
                "💉 SQL injection tip: never show detailed database error messages to normal users because they can help attackers learn how the system works.",
                "💉 SQL injection tip: database accounts should use least privilege, so a vulnerable form cannot easily damage the whole database.",
            },
            ["zero day"] = new List<string>
            {
                "🕳️ Zero-day tip: reduce risk by keeping security tools active, limiting unnecessary software, and avoiding suspicious files or links.",
                "🕳️ Zero-day tip: backups and least-privilege accounts help reduce damage even before a patch exists.",
            },
            ["patches"] = new List<string>
            {
                "🧩 Patching tip: turn on automatic updates for Windows, browsers, phone apps, antivirus tools, and router firmware where possible.",
                "🧩 Patching tip: restart your device when updates ask for it, because some security fixes only apply after restart.",
                "🧩 Patching tip: criminals often target people who delay updates after a security fix has been released.",
            },
            ["sql-zero-patching"] = new List<string>
            {
                "🛡 Combined tip: use secure coding to prevent SQL injection, layered protection for zero-day risk, and automatic updates to close known weaknesses quickly.",
                "🛡 Combined tip: SQL injection is prevented mainly by developers, zero-days are reduced through layered defences, and patching is controlled by keeping software updated.",
                "🛡 Combined tip: a strong system validates input, limits database permissions, monitors unusual activity, and applies security patches quickly.",
            },
            ["general"] = new List<string>
            {
                "🛡 General tip: think before you click. A few seconds of checking can prevent a serious cyber incident.",
                "🛡 General tip: use updates, backups, 2FA, and strong passwords together. Cybersecurity works best in layers.",
                "🛡 General tip: if a message creates panic or pressure, pause and verify it independently.",
            },
        };

        private readonly Dictionary<string, string> _generalResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["how are you"] = "I'm running smoothly and ready to help you stay cyber-safe. What topic would you like to explore today?",
            ["what is your purpose"] = "My purpose is to teach cybersecurity awareness in a friendly way, especially around phishing, passwords, scams, privacy, malware, and safe online behaviour.",
            ["what's your purpose"] = "My purpose is to teach cybersecurity awareness in a friendly way, especially around phishing, passwords, scams, privacy, malware, and safe online behaviour.",
            ["who are you"] = "I'm your Cybersecurity Awareness Assistant, a chatbot built to help South African citizens recognise and avoid online threats.",
            ["hello"] = "Hello! Great to have you here. Ask me about phishing, passwords, privacy, ransomware, or type 'what can I ask you about'.",
            ["hi"] = "Hi there! Ready to boost your cyber-safety knowledge? Ask me anything about online safety.",
            ["help"] = "Type 'what can I ask you about' for the full list, or ask naturally, such as 'tell me about phishing' or 'give me a password tip'.",
            ["thank you"] = "You're most welcome! Stay vigilant and safe online. 🛡",
            ["thanks"] = "Glad I could help! Cybersecurity is a daily habit, not a once-off task.",
            ["bye"] = "Stay safe online! Remember to think before you click. Goodbye! 👋",
            ["goodbye"] = "It was a pleasure chatting with you. Keep your accounts secure and browse safely!",
            ["exit"] = "Exiting the chatbot. Stay cyber-safe, and come back any time you have questions!",
            ["quit"] = "Thanks for chatting. Remember: think before you click, use strong passwords, and enable 2FA. Goodbye! 👋",
            ["end"] = "Thanks for chatting. Remember: think before you click, use strong passwords, and enable 2FA. Goodbye! 👋",
            ["what can i ask you about"] =
                "You can ask me about:\n" +
                "   • 🔑 Password safety and password managers\n" +
                "   • 🎣 Phishing, smishing, vishing, and fake links\n" +
                "   • 🌐 Safe browsing, HTTPS, and suspicious websites\n" +
                "   • 🔐 Two-factor authentication / MFA / OTP safety\n" +
                "   • 🏛️ CIA Triad: confidentiality, integrity, and availability\n" +
                "   • 🦠 Malware, viruses, trojans, spyware, and keyloggers\n" +
                "   • 💣 Ransomware and backups\n" +
                "   • ⚠️ Online scams and social engineering\n" +
                "   • 📶 Wi-Fi security, VPNs, and firewalls\n" +
                "   • 🔒 Privacy, POPIA, and data breaches\n" +
                "   • 🪪 Identity theft and SIM swap fraud\n" +
                "   • 💉 SQL injection, zero-day attacks, and patching\n" +
                "   • 📣 Reporting cybercrime and saving evidence\n\n" +
                "You can also ask: 'tell me more', 'give examples', or 'give me another tip' after a topic.",
        };

        public string? GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string lower = input.ToLower();

            string? randomResponse = GetRandomTipResponse(lower);
            if (randomResponse != null) return randomResponse;

            // Match cybersecurity topics before short general greetings.
            // This prevents words like "patching" from accidentally matching "hi".
            string? combinedTopicResponse = GetCombinedTopicResponse(lower);
            if (combinedTopicResponse != null) return combinedTopicResponse;

            string? topic = DetectTopic(lower);
            if (topic == "sql-zero-patching") return GetCombinedTopicResponse(lower);
            if (topic != null && _topicResponses.ContainsKey(topic)) return _topicResponses[topic];

            string? generalResponse = GetGeneralResponse(lower);
            if (generalResponse != null) return generalResponse;

            return null;
        }

        private string? GetGeneralResponse(string lower)
        {
            foreach (var pair in _generalResponses)
            {
                string key = pair.Key.ToLower();

                // Longer commands can be found inside the sentence.
                if (key.Length > 3 && lower.Contains(key)) return pair.Value;

                // Short greetings such as "hi" must match as a full word only.
                if (Regex.IsMatch(lower, $@"\b{Regex.Escape(key)}\b")) return pair.Value;
            }

            return null;
        }

        private string? GetCombinedTopicResponse(string lower)
        {
            if (IsSqlZeroDayPatchingTopic(lower))
            {
                return "💉 SQL injection, zero-day attacks, and patching are three important security topics:\n" +
                       "\n1. SQL injection\n" +
                       "   • This happens when attackers type malicious database commands into input fields, search boxes, login forms, or URL parameters.\n" +
                       "   • The goal is usually to view, change, delete, or steal information from a database.\n" +
                       "   • Developers prevent it by using parameterised queries, input validation, least-privilege database accounts, and secure error handling.\n" +
                       "\n2. Zero-day attacks\n" +
                       "   • A zero-day is a software weakness that attackers know about before the vendor has released a fix.\n" +
                       "   • These attacks are dangerous because there may be no patch available at first.\n" +
                       "   • Users reduce risk by using reputable software, antivirus, firewalls, backups, and avoiding suspicious files or links.\n" +
                       "\n3. Patching and updates\n" +
                       "   • Patches are updates that fix bugs and security weaknesses.\n" +
                       "   • Once a patch is released, criminals often target people who delay updates.\n" +
                       "   • Turn on automatic updates for Windows, browsers, apps, phones, and antivirus tools.\n" +
                       "\n🛡 In simple terms: SQL injection targets databases, zero-day attacks target unknown weaknesses, and patching closes known weaknesses before criminals exploit them.";
            }

            return null;
        }

        private static bool IsSqlZeroDayPatchingTopic(string lower)
        {
            bool asksSql = lower.Contains("sql") || lower.Contains("sqli") || lower.Contains("database attack");
            bool asksZeroDay = lower.Contains("zero day") || lower.Contains("zero-day") || lower.Contains("unpatched vulnerability");
            bool asksPatching = lower.Contains("patch") || lower.Contains("patching") || lower.Contains("updates") || lower.Contains("software update");
            return asksSql && asksZeroDay && asksPatching;
        }

        public string? DetectTopic(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string lower = input.ToLower();

            if (IsSqlZeroDayPatchingTopic(lower))
                return "sql-zero-patching";

            // Prefer longer/more specific aliases first so "cia triad" wins over "cia".
            foreach (var topic in _topicAliases)
            {
                foreach (string alias in topic.Value.OrderByDescending(a => a.Length))
                {
                    if (lower.Contains(alias.ToLower())) return topic.Key;
                }
            }
            return null;
        }

        private string? GetRandomTipResponse(string lower)
        {
            if (lower.Contains("another tip") || lower.Contains("random tip") || lower.Contains("give me a tip") || lower.Contains("cyber tip"))
            {
                string? topic = DetectTopic(lower);
                if (topic != null && _randomResponses.ContainsKey(topic)) return GetRandomFrom(topic);
                return GetRandomFrom("general");
            }

            if ((lower.Contains("tip") || lower.Contains("tips")) && lower.Contains("phish")) return GetRandomFrom("phishing");
            if ((lower.Contains("tip") || lower.Contains("tips")) && lower.Contains("password")) return GetRandomFrom("password");
            if ((lower.Contains("tip") || lower.Contains("tips")) && (lower.Contains("browsing") || lower.Contains("website") || lower.Contains("https"))) return GetRandomFrom("safe browsing");
            if ((lower.Contains("tip") || lower.Contains("tips")) && lower.Contains("scam")) return GetRandomFrom("scams");
            if ((lower.Contains("tip") || lower.Contains("tips")) && lower.Contains("privacy")) return GetRandomFrom("privacy");

            return null;
        }

        public string GetRandomTipForTopic(string? topic)
        {
            if (topic == null) return GetRandomFrom("general");
            string lower = topic.ToLower();
            if (lower.Contains("sql") && (lower.Contains("zero") || lower.Contains("patch"))) return GetRandomFrom("sql-zero-patching");
            if (lower.Contains("phishing")) return GetRandomFrom("phishing");
            if (lower.Contains("password")) return GetRandomFrom("password");
            if (lower.Contains("browsing")) return GetRandomFrom("safe browsing");
            if (lower.Contains("scam")) return GetRandomFrom("scams");
            if (lower.Contains("privacy")) return GetRandomFrom("privacy");
            if (lower.Contains("sql")) return GetRandomFrom("sql injection");
            if (lower.Contains("zero")) return GetRandomFrom("zero day");
            if (lower.Contains("patch") || lower.Contains("update")) return GetRandomFrom("patches");
            return GetRandomFrom("general");
        }

        private string GetRandomFrom(string key)
        {
            var pool = _randomResponses[key];
            return pool[_random.Next(pool.Count)];
        }

        public string? GetMoreForTopic(string topic)
        {
            string lower = topic.ToLower();
            if (lower.Contains("sql") && (lower.Contains("zero") || lower.Contains("patch")))
                return GetCombinedMoreExplanation();

            string? detectedTopic = DetectTopic(topic) ?? NormaliseMemoryTopic(topic);

            if (detectedTopic == "sql-zero-patching")
                return GetCombinedMoreExplanation();

            if (detectedTopic == null || !_topicResponses.ContainsKey(detectedTopic)) return null;

            return "Here is a deeper explanation:\n" + _topicResponses[detectedTopic] + "\n\n" + GetExtraDetail(detectedTopic);
        }

        private string? NormaliseMemoryTopic(string memoryTopic)
        {
            string lower = memoryTopic.ToLower();
            if (lower.Contains("sql") && (lower.Contains("zero") || lower.Contains("patch"))) return "sql-zero-patching";
            if (lower.Contains("password")) return "password";
            if (lower.Contains("phishing")) return "phishing";
            if (lower.Contains("privacy")) return "privacy";
            if (lower.Contains("ransomware")) return "ransomware";
            if (lower.Contains("malware")) return "malware";
            if (lower.Contains("vpn")) return "vpn";
            if (lower.Contains("factor") || lower.Contains("2fa")) return "two-factor authentication";
            if (lower.Contains("cia")) return "cia triad";
            if (lower.Contains("encryption")) return "encryption";
            if (lower.Contains("scam")) return "scams";
            if (lower.Contains("social")) return "social engineering";
            if (lower.Contains("firewall")) return "firewall";
            if (lower.Contains("backup")) return "backup";
            if (lower.Contains("sql")) return "sql injection";
            if (lower.Contains("zero day") || lower.Contains("zero-day")) return "zero day";
            if (lower.Contains("patch") || lower.Contains("update")) return "patches";
            if (lower.Contains("wi-fi") || lower.Contains("wifi")) return "wifi security";
            if (lower.Contains("identity")) return "identity theft";
            if (lower.Contains("breach")) return "data breach";
            if (lower.Contains("sim")) return "sim swap";
            if (lower.Contains("popia")) return "popia";
            return null;
        }

        private string GetExtraDetail(string topic)
        {
            switch (topic)
            {
                case "cia triad":
                    return "Example scenario: A university student portal needs confidentiality so only the student can see marks, integrity so marks cannot be changed without authorisation, and availability so students can access results when released.";
                case "phishing":
                    return "Best action plan: stop, inspect the sender, do not click, verify through official channels, report the message, and delete it after evidence is saved if needed.";
                case "password":
                    return "A strong habit is to secure your email first because email is often used to reset passwords for many other services.";
                case "privacy":
                    return "A useful privacy habit is to search your own name online occasionally and remove unnecessary public information where possible.";
                case "ransomware":
                    return "Backups should be tested. A backup that cannot restore your files is not useful during an emergency.";
                case "popia":
                    return "POPIA links strongly with cybersecurity because organisations cannot protect privacy if they fail to secure personal information.";
                default:
                    return "A good cybersecurity habit is to use layered protection: awareness, strong passwords, 2FA, updates, backups, and careful verification.";
            }
        }

        public string? GetExamplesForTopic(string topic)
        {
            string? detectedTopic = DetectTopic(topic) ?? NormaliseMemoryTopic(topic);
            if (detectedTopic == null) return null;

            if (detectedTopic == "sql-zero-patching")
                return GetCombinedExamples();

            switch (detectedTopic)
            {
                case "cia triad":
                    return "Examples of the CIA Triad:\n" +
                           "   🔒 Confidentiality: only authorised staff can view customer banking details.\n" +
                           "   ✅ Integrity: a bank balance must not change unless a valid transaction occurs.\n" +
                           "   ⚡ Availability: online banking must stay accessible when customers need it.\n" +
                           "   🛡 Real-world link: phishing can break confidentiality, data tampering breaks integrity, and ransomware breaks availability.";
                case "phishing":
                    return "Phishing examples:\n" +
                           "   🎣 A fake bank email says your account will be closed unless you log in immediately.\n" +
                           "   🎣 A fake delivery SMS asks for a customs payment through a suspicious link.\n" +
                           "   🎣 A fake Microsoft login page steals your school or work password.";
                case "password":
                    return "Password examples:\n" +
                           "   ✅ Stronger: Coffee!River!Planet!27\n" +
                           "   ❌ Weak: password123, qwerty, your name, or your birthday.\n" +
                           "   ✅ Safer habit: one unique password per account plus 2FA.";
                case "privacy":
                    return "Privacy examples:\n" +
                           "   ✅ Turn off unnecessary location access.\n" +
                           "   ✅ Keep social media profiles private.\n" +
                           "   ❌ Do not post your ID number, home address, boarding pass, or banking details.";
                case "scams":
                    return "Scam examples:\n" +
                           "   ⚠️ Fake job offer asking for an upfront registration fee.\n" +
                           "   ⚠️ Fake SARS refund link asking for bank details.\n" +
                           "   ⚠️ Fake marketplace buyer sending false proof of payment.";
                case "ransomware":
                    return "Ransomware examples:\n" +
                           "   💣 An employee opens a fake invoice attachment and company files become encrypted.\n" +
                           "   💣 A student downloads cracked software and loses access to assignments.\n" +
                           "   💣 A business cannot operate because important systems are locked.";
                case "two-factor authentication":
                    return "2FA examples:\n" +
                           "   🔐 Password + authenticator app code.\n" +
                           "   🔐 Password + fingerprint.\n" +
                           "   🔐 Password + hardware security key.\n" +
                           "   ⚠️ SMS OTP is better than no 2FA, but authenticator apps are safer.";
                case "malware":
                    return "Malware examples:\n" +
                           "   🦠 A Trojan disguised as a free game.\n" +
                           "   🦠 Spyware that monitors browsing activity.\n" +
                           "   🦠 A keylogger that records passwords as you type.";
                default:
                    return "Examples for this topic:\n" + _topicResponses.GetValueOrDefault(detectedTopic, "Use strong passwords, verify messages, update software, and back up important data.");
            }
        }

        private string GetCombinedMoreExplanation()
        {
            return "Here is a deeper explanation of SQL injection, zero-day attacks, and patching:\n" +
                   "\n1. SQL injection\n" +
                   "   • SQL injection is mainly a developer-side security problem in web applications.\n" +
                   "   • It happens when the application places user input directly into a database query.\n" +
                   "   • A secure system uses parameterised queries, validation, least-privilege database accounts, and safe error messages.\n" +
                   "\n2. Zero-day attacks\n" +
                   "   • A zero-day attack targets a weakness before a public fix is available.\n" +
                   "   • Because there may be no patch at first, layered defence becomes important: antivirus, firewalls, least privilege, backups, and careful behaviour.\n" +
                   "   • Organisations also monitor unusual activity so they can react quickly.\n" +
                   "\n3. Patching and updates\n" +
                   "   • Patching fixes known weaknesses after vendors release updates.\n" +
                   "   • Delaying updates gives criminals more time to attack known vulnerabilities.\n" +
                   "   • Automatic updates are one of the easiest ways for normal users to stay safer.\n" +
                   "\n🛡 How they connect: SQL injection is prevented by secure coding, zero-day risk is reduced through layered defence, and patching closes known weaknesses as soon as fixes are available.";
        }

        private string GetCombinedExamples()
        {
            return "Examples for SQL injection, zero-day attacks, and patching:\n" +
                   "   💉 SQL injection: a login form is poorly coded, and an attacker types database commands to bypass authentication or view private records.\n" +
                   "   🕳️ Zero-day attack: criminals exploit a newly discovered browser weakness before the vendor has released a fix.\n" +
                   "   🧩 Patching: Windows, Chrome, antivirus software, phone apps, and router firmware receive updates that close known security holes.\n" +
                   "   🛡 Combined scenario: a company prevents SQL injection in its website, monitors for unknown attacks, and installs patches quickly once updates are released.";
        }

        public IEnumerable<string> GetSupportedTopics()
        {
            return _topicResponses.Keys;
        }
    }
}

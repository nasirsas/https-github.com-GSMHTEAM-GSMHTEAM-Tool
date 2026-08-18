# 🔧 GSMHTEAM Tool v6.1 - Professional Mobile Device Management

**An all-in-one Flask-based workstation for managing Android devices with factory resets, FRP bypass, bootloader operations, firmware flashing, and intelligent backup/restore capabilities.**

---

## ✨ Features

### Core Operations
- 📱 **Device Detection** - Auto-detect connected devices via ADB/Fastboot
- 🔧 **Factory Reset** - Complete device wipe with secure bootloader unlock
- 🔓 **FRP Bypass** - Factory Reset Protection removal
- 🔑 **Bootloader Management** - Lock/Unlock bootloader operations
- 📤 **Firmware Flashing** - Multi-partition firmware updates
- 💾 **Smart Backup & Restore** - Intelligent partition backups with compression

### Advanced Features ⭐ NEW
- 📊 **Real-time Device Monitoring** - Live device status dashboard
- 🎯 **Batch Operations** - Process multiple devices simultaneously
- 🔐 **Advanced Anti-Crack** - Hardware-locked licensing system
- 💳 **Credit-Based System** - Granular operation cost control
- 📈 **Analytics Dashboard** - Operation history, success rates, performance metrics
- 🌐 **Multi-Language Support** - WebSocket-powered real-time UI updates
- 📋 **Operation Logs** - Detailed audit trail with timestamps
- 🔔 **Smart Notifications** - Real-time alerts via WebSocket

### Supported Devices
- **TECNO** - Vision, Spark, Camon series
- **Infinix** - Vision, Hot, Zero, Note series
- **Itel** - Vision, A-series
- **Nokia** - C-series, Snapdragon devices
- **Xiaomi** - Redmi Note, Xiaomi flagship series
- **Realme** - GT, 12 Pro+ series
- **Oppo** - Find X, Reno series
- **Vivo** - X, V series
- **Samsung** - Galaxy S24, S23 series
- **Motorola** - Moto G series

---

## 🚀 Quick Start

### Prerequisites
```bash
python3 >= 3.8
pip install flask flask-socketio flask-cors flask-limiter werkzeug
adb (Android Debug Bridge)
fastboot (Fastboot utility)
```

### Installation
```bash
git clone https://github.com/nasirsas/https-github.com-GSMHTEAM-GSMHTEAM-Tool.git
cd https-github.com-GSMHTEAM-GSMHTEAM-Tool
pip install -r requirements.txt
python3 gsmhteam.py
```

### Access
```
🌐 Web Interface: http://localhost:5000
👤 Default Login: admin / admin123
💰 Free Credits: 1,000,000
```

---

## 📊 API Endpoints

### Authentication
```
POST   /api/login                 - User login
GET    /api/logout                - User logout
GET    /api/me                    - Current user info
```

### Device Operations
```
GET    /api/devices/detect        - Detect connected devices
GET    /api/device/<serial>/info  - Get device information
POST   /api/operation/<type>      - Execute operation (factory_reset, frp_bypass, etc.)
```

### Backup & Restore
```
POST   /api/backup/create         - Create device backup
POST   /api/backup/restore        - Restore backup to device
GET    /api/backup/list           - List all backups
GET    /api/backup/download/<id>  - Download backup file
DELETE /api/backup/delete/<id>    - Delete backup
```

### Credits & Licensing
```
GET    /api/credits/balance       - Get credit balance
GET    /api/credits/packages      - Get credit packages
POST   /api/credits/purchase      - Purchase credits
GET    /api/license/status        - Check license status
POST   /api/license/activate      - Activate license key
```

### Analytics
```
GET    /api/stats                 - User statistics
GET    /api/supported-models      - List supported models
GET    /api/operations/history    - Operation history [NEW]
GET    /api/device/<serial>/status - Real-time device status [NEW]
```

---

## 💳 Credit System

### Operation Costs
| Operation | Cost |
|-----------|------|
| Device Detection | 1 |
| Read Device Info | 5 |
| FRP Bypass | 30 |
| Factory Reset | 15 |
| Bootloader Unlock | 35 |
| Bootloader Lock | 20 |
| Firmware Flash | 60 |
| Create Backup | 20 |
| Restore Backup | 20 |

### Credit Packages
| Package | Credits | Price |
|---------|---------|-------|
| Starter | 100 | $9.99 |
| Pro | 500 | $39.99 |
| Enterprise | 2000 | $149.99 |
| VIP | 5000 | $299.99 |

---

## 🔐 Security Features

### Anti-Crack Protection
- ✅ Hardware-locked licensing (CPU ID + MAC address)
- ✅ Trial mode with automatic expiration (7 days default)
- ✅ License key validation and expiry checking
- ✅ Crack detection with security alerts
- ✅ Session-based authentication with CSRF protection

### Rate Limiting
- ✅ 100 requests per minute per IP
- ✅ 10 login attempts per minute
- ✅ Secure password hashing (SHA-256)
- ✅ CORS protection with credential support

### Data Protection
- ✅ SQLite database with proper prepared statements
- ✅ Backup encryption with ZIP deflate compression
- ✅ Secure temporary file handling
- ✅ Audit logs for all operations

---

## 🛠️ Configuration

Edit the `Config` class in `gsmhteam.py`:

```python
class Config:
    SECRET_KEY = secrets.token_hex(64)      # Session encryption
    VERSION = "6.1"                          # Current version
    BACKUP_DIR = './backups'                 # Backup storage location
    TRIAL_DAYS = 7                           # Trial period length
    MAX_BACKUP_SIZE = 5 * 1024**3            # Max backup size (5GB)
    LOG_RETENTION_DAYS = 90                  # Log retention period
```

---

## 📈 Performance Tips

1. **Batch Operations** - Process multiple devices to optimize credits
2. **Backup Scheduling** - Create backups during off-peak hours
3. **Database Cleanup** - Regularly delete old backups
4. **Connection Pool** - Reuse database connections
5. **Rate Limiting** - Adjust per your infrastructure needs

---

## 🔄 Updates & Changelog

### v6.1 (Current)
- ✅ Fixed syntax error in `credit_packages()` route
- ✅ Implemented missing `is_cracked()` method
- ✅ Implemented missing `activate_license()` method
- ✅ Implemented missing `delete_backup()` method
- ✅ Fixed subprocess device targeting
- ✅ Added real-time device monitoring [NEW]
- ✅ Added batch operations support [NEW]
- ✅ Added operation history endpoint [NEW]
- ✅ Enhanced error handling and logging [NEW]
- ✅ Improved UI/UX with better responsiveness [NEW]

### v6.0
- Initial release with core features

---

## 📞 Support

- **Email**: support@gsmhteam.com
- **Issues**: Create an issue on GitHub
- **Community**: Discord server (link TBA)

---

**Made with ❤️ by GSMHTEAM**

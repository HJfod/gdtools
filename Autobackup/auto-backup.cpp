#include <iostream>
#include <fstream>
#include <string>
#include <ShlObj.h>
#include <sstream>
#include <tchar.h>
#include <windows.h>
#include <psapi.h>
#include <winuser.h>
#include <thread>
#include <chrono>
#include <ctime>
#include <time.h>
#include <direct.h>
//#include "zipper/zipper.h"

#define GD_NOT_OPEN                     5
#define GD_IS_OPEN                      6

#define ERR_GET_PROCESS_LIST            10
#define ERR_NO_SRC_DIR_SET              11
#define ERR_NO_DEST_DIR_SET             12
#define ERR_SRC_DOESNT_EXIST            13
#define ERR_DEST_DOESNT_EXIST           14
#define ERR_SETTINGS_FILE_NOT_FOUND     2
#define ERR_UNKNOWN                     3
#define ERR_INSUFFICIENT_SETTINGS       4

#define BACKUP_SUCCESS                  20
#define ERR_CCLOCALLEVELS_NOT_FOUND     21
#define ERR_CCGAMEMANAGER_NOT_FOUND     22
#define ERR_COPY_FILE_FROM_NOT_FOUND    25
#define COPY_SUCCESS                    26
#define ERR_BACKUP_CANT_CREATE_DIR      23
#define ERR_BACKUP_CANT_CREATE_DEL_FILE 27

#define DEBUG false
#define VERSION "v1.0"
#define LOGFILE ".errlog"
#define BACKUPTEXT "GDTOOLS_BACKUP_"
#define DELETEFILE "autoRemove.txt"
#define COMPRESSEXT "gdb"

// clang++ auto-backup.cpp -o ab.exe -lshell32 -lole32

const std::string deleteText(std::string _am) {
    return "This file will be deleted once your set limit of " + _am + " backups has been reached.\n\nIf you'd like to preserve this backup, delete this text file.";
}

std::string GetCCPath(std::string WHICH = "LocalLevels", std::string PATH = "") {
    if (PATH != "") return PATH + "\\CC" + WHICH + ".dat";

    wchar_t* localAppData = 0;
    SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &localAppData);

    std::wstring CCW (localAppData);

    std::string RESULT ( CCW.begin(), CCW.end() );
    RESULT += "\\GeometryDash\\CC" + WHICH + ".dat";

    CoTaskMemFree(static_cast<void*>(localAppData));
    
    return RESULT;
}

inline bool FileDirExists (const std::string& name) {
  struct stat buffer;
  return (stat (name.c_str(), &buffer) == 0); 
}

void writeLog(std::string _log) {
    std::ofstream logfile (LOGFILE);
    if (logfile.is_open()) {
        logfile << _log;
        logfile.close();
    }
}

void quit(int _CODE) {
    writeLog("Application quit with error code " + std::to_string(_CODE) +
    "\nCheck https://github.com/HJfod/gdtools/blob/master/Autobackup/ErrorCodes.md for details");
    std::cout << "Error code: " << _CODE << std::endl;
    std::exit(_CODE);
}

namespace sett {
    std::string src = "";
    std::string dest = "";
    bool enabled = false;
    unsigned int rate = 0;
    unsigned int limit = 1;
    unsigned long lastbackup = 0;
    bool create_on_gd_close = 0;
    bool compress = 0;
    unsigned int gd_check_rate = 10;
    bool debug = 0;
    unsigned int gd_check_length = 3;
    std::string compressed_ext = COMPRESSEXT;
}

enum SETT_CODES {
    SRCDIR, DESTDIR, ENABLED, RATE, LIMIT, LAST, ONCLOSE, COMPRESS, CHECKRATE, DEBUGMODE, CHECKLENGTH, COMPEXT
};

int GetCode(std::string _line) {
    if (_line == "src")                 return SRCDIR;
    if (_line == "dest")                return DESTDIR;
    if (_line == "enabled")             return ENABLED;
    if (_line == "rate")                return RATE;
    if (_line == "limit")               return LIMIT;
    if (_line == "lastbackup")          return LAST;
    if (_line == "create_on_gd_close")  return ONCLOSE;
    if (_line == "compress")            return COMPRESS;
    if (_line == "gd_check_rate")       return CHECKRATE;
    if (_line == "debug")               return DEBUGMODE;
    if (_line == "gd_check_length")     return CHECKLENGTH;
    if (_line == "compress_ext")        return COMPEXT;

    return 0;
}

std::string GetProcessName(DWORD processID) {
    TCHAR szProcessName[MAX_PATH] = TEXT("<unknown>");

    HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, processID);

    if (hProcess != NULL) {
        HMODULE hMod;
        DWORD cbNeeded;

        if (EnumProcessModules(hProcess, &hMod, sizeof(hMod), &cbNeeded))
            GetModuleBaseName(hProcess, hMod, szProcessName, sizeof(szProcessName)/sizeof(TCHAR));
    }

    CloseHandle(hProcess);

    std::basic_string<TCHAR> w = szProcessName;
    return std::string(w.begin(), w.end());
}

int CopyFile(std::string from, std::string to) {
    if (!FileDirExists(from))
        quit(ERR_COPY_FILE_FROM_NOT_FOUND);
    std::ifstream src(from, std::ios::binary);
    std::ofstream dst(to,   std::ios::binary);
    dst << src.rdbuf();

    return COPY_SUCCESS;
}

int CheckBackupsAmount() {
    return 0;
}

int CreateBackup(std::string *ret) {
    std::string ccll = GetCCPath("LocalLevels", sett::src);
    std::string ccgm = GetCCPath("GameManager", sett::src);

    if (!FileDirExists(ccll)) quit(ERR_CCLOCALLEVELS_NOT_FOUND);
    if (!FileDirExists(ccgm)) quit(ERR_CCGAMEMANAGER_NOT_FOUND);

    std::time_t t = std::time(0);
    std::tm now;
    localtime_s(&now, &t);
    std::stringstream nams;
    nams << sett::dest << "\\" << BACKUPTEXT
    << (now.tm_year + 1900) << '-' << (now.tm_mon + 1) << '-' <<  now.tm_mday;
    std::string backuppath = nams.str();

    if (_mkdir(backuppath.c_str()) != 0)
        quit(ERR_BACKUP_CANT_CREATE_DIR);
    
    std::ofstream delfile (backuppath + "\\" + DELETEFILE);
    if (delfile.is_open()) {
        delfile << deleteText(std::to_string(sett::limit));
        delfile.close();
    } else quit(ERR_BACKUP_CANT_CREATE_DEL_FILE);

    CopyFile(ccll, backuppath + "\\CCLocalLevels.dat");
    CopyFile(ccgm, backuppath + "\\CCGameManager.dat");

    /*
    if (sett::compress) {
        std::ifstream gmf(backuppath + "\\CCLocalLevels.dat");
        std::ifstream llf(backuppath + "\\CCGameManager.dat");

        zipper::Zipper zipper(backuppath + "." + sett::compressed_ext);
        zipper.add(gmf, "CCGameManager.dat");
        zipper.add(llf, "CCLocalLevels.dat");
        zipper.close();

        std::system(("rd /s /q " + backuppath).c_str());

        *ret = backuppath + "." + sett::compressed_ext;
    } else */
    
    *ret = backuppath;

    return BACKUP_SUCCESS;
}

int CheckGDOpen() {
    DWORD proc[1024], cb;
    if (!EnumProcesses(proc, sizeof(proc), &cb)) return ERR_GET_PROCESS_LIST;

    for (int i = 0; i < cb / sizeof(DWORD); i++)
        if (proc[i] != 0)
            if (GetProcessName(proc[i]) == "GeometryDash.exe")
                return GD_IS_OPEN;
    
    return GD_NOT_OPEN;
}

void GDOnCloseBackuper() {
    unsigned int makeOnNext = 0;
    while (sett::create_on_gd_close) {
        switch (CheckGDOpen()) {
            case GD_IS_OPEN:
                makeOnNext += 1;
                break;
            case GD_NOT_OPEN:
                if (makeOnNext >= sett::gd_check_length) {
                    std::cout << "Creating backup..." << std::endl;
                    std::string succ;
                    switch (CreateBackup(&succ)) {
                        case BACKUP_SUCCESS:
                            std::cout << "Succesfully made backup!" << std::endl << succ << std::endl;
                            break;
                        default:
                            quit(ERR_UNKNOWN);
                    }
                }
                break;
            default:
                quit(ERR_GET_PROCESS_LIST);
        }

        std::this_thread::sleep_for(std::chrono::seconds(sett::gd_check_rate));
    }
}

int main() {
    std::string line;
    std::ifstream settings_file (".autobackup");

    std::cout << "GDTools Autobackup " << VERSION << std::endl;
    std::cout << "Developed by HJfod" << std::endl;
    std::cout << "Loading .autobackup " << (DEBUG ? "[DEBUG ON]" : "") << "..." << std::endl;

    std::chrono::steady_clock::time_point time;
    #ifdef DEBUG
        time = std::chrono::high_resolution_clock::now();
    #endif

    if (settings_file.is_open()) {
        while ( getline (settings_file, line) ) {
            std::string val = line.substr(line.find(" ") + 1);

            switch (GetCode(line.substr(0, line.find(" ")))) {
                case SRCDIR:        sett::src = val; break;
                case DESTDIR:       sett::dest = val; break;
                case ENABLED:       sett::enabled = val == "1"; break;
                case RATE:          sett::rate = std::stoi(val); break;
                case LIMIT:         sett::limit = std::stoi(val); break;
                case LAST:          sett::lastbackup = std::stol(val); break;
                case ONCLOSE:       sett::create_on_gd_close = val == "1"; break;
                case COMPRESS:      sett::compress = val == "1"; break;
                case CHECKRATE:     sett::gd_check_rate = std::stoi(val); break;
                case DEBUGMODE:     sett::debug = val == "1"; break;
                case CHECKLENGTH:   sett::gd_check_length = std::stoi(val); break;
                case COMPEXT:       sett::compressed_ext = val; break;
            }
        }
        settings_file.close();
    } else quit(ERR_SETTINGS_FILE_NOT_FOUND);

    if (!sett::enabled)
        return 0;

    //if (sett::src.empty()) quit(ERR_NO_SRC_DIR_SET);
    if (sett::dest.empty()) quit(ERR_NO_DEST_DIR_SET);
    if (!FileDirExists(sett::src.empty() ? GetCCPath() : sett::src)) quit(ERR_SRC_DOESNT_EXIST);
    if (!FileDirExists(sett::dest)) quit(ERR_DEST_DOESNT_EXIST);

    if (DEBUG || sett::debug) {
        if (DEBUG) std::cout << std::endl << "Loaded in " << (std::chrono::high_resolution_clock::now() - time).count() << "ns" << std::endl;

        std::cout << "Settings" << std::endl;
        std::cout << "src: "                << sett::src << std::endl;
        std::cout << "dest: "               << sett::dest << std::endl;
        std::cout << "enabled: "            << sett::enabled << std::endl;
        std::cout << "rate: "               << sett::rate << std::endl;
        std::cout << "limit: "              << sett::limit << std::endl;
        std::cout << "lastbackup: "         << sett::lastbackup << std::endl;
        std::cout << "create_on_gd_close: " << sett::create_on_gd_close << std::endl;
        std::cout << "compress: "           << sett::compress << std::endl;
        std::cout << "gd_check_rate: "      << sett::gd_check_rate << std::endl;
        std::cout << "gd_check_length: "    << sett::gd_check_length << std::endl;
        std::cout << "debug: "              << sett::debug << std::endl << std::endl;
    } else {
        std::cout << "Backups directory: " << sett::dest << std::endl << std::endl;
    }

    if (sett::create_on_gd_close) {
        std::thread thr(GDOnCloseBackuper);
        thr.join();
    }

    return 0;
}
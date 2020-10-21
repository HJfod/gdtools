#include <iostream>
#include <fstream>
#include <string>
#include <sstream>
#include <tchar.h>
#include <windows.h>
#include <psapi.h>
#include <winuser.h>
#include <thread>
#include <chrono>
#include <ctime>
#include <time.h>

#define GD_NOT_OPEN                     5
#define GD_IS_OPEN                      6

#define ERR_GET_PROCESS_LIST            10
#define ERR_NO_SRC_DIR_SET              11
#define ERR_NO_DEST_DIR_SET             12
#define ERR_SETTINGS_FILE_NOT_FOUND     2

#define BACKUP_SUCCESS                  20

#define DEBUG true

void quit(int _CODE) {
    std::cout << "Error code: " << _CODE << std::endl;
    std::exit(_CODE);
}

namespace sett {
    std::string src;
    std::string dest;
    bool enabled;
    unsigned int rate;
    unsigned int limit;
    unsigned long lastbackup = 0;
    bool create_on_gd_close = 0;
    bool compress = 0;
    unsigned int gd_check_rate = 10;
    bool debug = 0;
    unsigned int gd_check_length = 1;
}

enum SETT_CODES {
    SRCDIR, DESTDIR, ENABLED, RATE, LIMIT, LAST, ONCLOSE, COMPRESS, CHECKRATE, DEBUGMODE, CHECKLENGTH
};

int GET_CODE(std::string _line) {
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

int CreateBackup() {


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
                    switch (CreateBackup()) {
                        case BACKUP_SUCCESS:
                            std::time_t time = std::time(NULL);
                            char time_str[26];
                            std::cout << "Succesfully made backup at " << ctime_s(time_str, sizeof time_str, &time) << std::endl;
                            break;
                    }
                    makeOnNext = 0;
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

    std::cout << "Loading .autobackup " << (DEBUG ? "[DEBUG ON]" : "") << "..." << std::endl << std::endl;

    std::chrono::steady_clock::time_point time;
    #ifdef DEBUG
        time = std::chrono::high_resolution_clock::now();
    #endif

    if (settings_file.is_open()) {
        while ( getline (settings_file, line) ) {
            std::string val = line.substr(line.find(" ") + 1);

            switch (GET_CODE(line.substr(0, line.find(" ")))) {
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
            }
        }
        settings_file.close();
    } else quit(ERR_SETTINGS_FILE_NOT_FOUND);

    if (sett::src.empty()) quit(ERR_NO_SRC_DIR_SET);
    if (sett::dest.empty()) quit(ERR_NO_DEST_DIR_SET);

    if (DEBUG || sett::debug) {
        if (DEBUG) std::cout << "Loaded in " << (std::chrono::high_resolution_clock::now() - time).count() << "ns" << std::endl;

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
    }

    if (sett::create_on_gd_close) {
        std::thread thr(GDOnCloseBackuper);
        thr.join();
    }

    return 0;
}
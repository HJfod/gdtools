#include <iostream>
#include <fstream>
#include <string>
#include <sstream>
#include <tchar.h>
#include <windows.h>
#include <psapi.h>
#include <winuser.h>
#include <thread>

#define ERR_GET_PROCESS_LIST 10
#define GD_NOT_OPEN 5
#define GD_IS_OPEN 6

#define DEBUG true

namespace sett {
    std::string src;
    std::string dest;
    bool enabled;
    unsigned int rate;
    unsigned int limit;
    unsigned long lastbackup;
    bool create_on_gd_close;
    bool compress;
    unsigned int gd_check_rate = 10;
}

enum SETT_CODES {
    SRCDIR, DESTDIR, ENABLED, RATE, LIMIT, LAST, ONCLOSE, COMPRESS, CHECKRATE
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
    while (sett::create_on_gd_close) {
        bool makeOnNext = false;
        int c = CheckGDOpen();
        if (c == GD_IS_OPEN)
            makeOnNext = true;

        std::this_thread::sleep_for(std::chrono::seconds(sett::gd_check_rate));
    }
}

int main() {
    std::string line;
    std::ifstream myfile (".autobackup");

    std::cout << "Loading .autobackup " << DEBUG ? "[DEBUG ON]" : "" << "..." << std::endl;

    if (myfile.is_open()) {
        while ( getline (myfile, line) ) {
            std::string val = line.substr(line.find(" ") + 1);

            switch (GET_CODE(line.substr(0, line.find(" ")))) {
                case SRCDIR:    sett::src = val; break;
                case DESTDIR:   sett::dest = val; break;
                case ENABLED:   sett::enabled = val == "1"; break;
                case RATE:      sett::rate = std::stoi(val); break;
                case LIMIT:     sett::limit = std::stoi(val); break;
                case LAST:      sett::lastbackup = std::stol(val); break;
                case ONCLOSE:   sett::create_on_gd_close = val == "1"; break;
                case COMPRESS:  sett::compress = val == "1"; break;
                case CHECKRATE: sett::gd_check_rate = std::stoi(val); break;
            }
        }
        myfile.close();
    } else std::cout << "Unable to open file!" << std::endl;

    if (DEBUG)
        std::cout << "Settings" << std::endl << std::endl;

    if (sett::create_on_gd_close) {
        std::thread thr(GDOnCloseBackuper);
        thr.join();
    }

    return 0;
}
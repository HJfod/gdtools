#include <iostream>
#include <fstream>
#include <string>
#include "wintoastlib.h"

using namespace WinToastLib;

int main() {   
    if (WinToast::isCompatible()) {
        std::wcout << L"Error, your system in not supported!" << std::endl;
    }

    WinToast::instance()->setAppName(L"GDTools");
    const auto aumi = WinToast::configureAUMI(L"HJfod", L"gdtools", L"gdtools", L"20200927");
    WinToast::instance()->setAppUserModelId(aumi);

    if (!WinToast::instance()->initialize()) {
        std::wcout << L"Error, could not initialize the lib!" << std::endl;
    }
    
    class WinToastHandlerExample : public IWinToastHandler {
    public:
        WinToastHandlerExample();
        void toastActivated() const override;
        void toastDismissed(WinToastDismissalReason state) const override;
        void toastFailed() const override;
    };
    
    //WinToastHandlerExample* handler = new WinToastHandlerExample;
    WinToastTemplate templ = WinToastTemplate(WinToastTemplate::ImageAndText02);
    templ.setImagePath(L"../Resources/gdtools.png");
    templ.setTextField(L"title", WinToastTemplate::FirstLine);
    templ.setTextField(L"subtitle", WinToastTemplate::SecondLine);

    /*if (!WinToast::instance()->showToast(templ, handler)) {
        std::wcout << L"Error: Could not launch your toast notification!" << std::endl;
    }*/

    std::string line;
    std::ifstream myfile (".autobackup");
    if (myfile.is_open()) {
        while ( getline (myfile,line) ) {
            std::cout << line << '\n';
        }
        myfile.close();
    } else std::cout << "Unable to open file!" << std::endl;

    return 0;
}
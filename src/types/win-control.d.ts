declare module 'win-control' {
    export class Window {
        static getByPid: (PID: number) => Window | undefined
        static getForeground: () => Window
        static getByTitle: (title: string) => Window | undefined
        constructor(HWND: number)
        getParent: () => Window | undefined
        getAncestor: (kindOfAncestor: AncestorFlags) => Window | undefined
        getProcessInfo: () => {
            windowText: string
            pid: number
            path: string
        }
        getPid: () => number
        getClassName: () => string
        getTitle: () => string
        exists: () => boolean
        isVisible: () => boolean
        getDimensions: () => {
            left: number
            top: number
            right: number
            bottom: number
        }
        getHwnd: () => number
        moveRelative: (
            dx: number,
            dy: number,
            dw: number,
            dh: number,
        ) => boolean
        setShowStatus: (state: WindowStates) => boolean
        setPosition: (
            hwndInsertAfter: number,
            x: number,
            y: number,
            cx: number,
            cy: number,
            uFlags: SWP
        ) => boolean
        setForeground: () => boolean
        close: () => boolean
    }

    export enum AncestorFlags {
        PARENT,
        ROOT,
        ROOTOWNER,
    }

    export enum WindowStates {
        HIDE,
        SHOWNORMAL,
        SHOWMINIMIZED,
        MAXIMIZE,
        SHOWMAXIMIZED,
        SHOWNOACTIVATE,
        SHOW,
        MINIMIZE,
        SHOWMINNOACTIVE,
        SHOWNA,
        RESTORE,
        SHOWDEFAULT,
        FORCEMINIMIZE
    }
    
    export enum HWND {
        NOTOPMOST,
        TOPMOST,
        TOP,
        BOTTOM
    }
    
    export enum SWP {
        NOSIZE,
        NOMOVE,
        NOZORDER,
        NOREDRAW,
        NOACTIVATE,
        DRAWFRAME,
        FRAMECHANGED,
        SHOWWINDOW,
        HIDEWINDOW,
        NOCOPYBITS,
        NOOWNERZORDER,
        NOREPOSITION,
        NOSENDCHANGING,
        DEFERERASE,
        ASYNCWINDOWPOS
    }
}
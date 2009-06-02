//////////////////////////////////////////////////////////////////////////////////////////////////
// File: DirectInput.h
// Author: Chris Smith
// Date Created: 2/22/06
// Description: Holds all of the Dirct Input data
// Copyright: Use this however you want, but I am not responsible for anything
//////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once
#include <dinput.h>
#include <d3dx9.h>

class DirectInput
{
public:
	DirectInput();
	~DirectInput();

	IDirectInput8*       DInput;
	IDirectInputDevice8* Keyboard;
	char                 KeyboardState[256]; 
	IDirectInputDevice8* Mouse;
	DIMOUSESTATE2        MouseState;

	void InitializeDirectInput(HINSTANCE appInstance, HWND hwnd, 
		                       DWORD keyboardCoopFlags, DWORD mouseCoopFlags);

	void Poll();
	bool KeyDown(char key);
	bool MouseButtonDown(int button);
};
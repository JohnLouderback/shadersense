//////////////////////////////////////////////////////////////////////////////////////////////////
// File: DirectInput.cpp
// Author: Chris Smith
// Date Created: 2/22/06
// Description: Holds all of the Dirct Input data
// Copyright: Use this however you want, but I am not responsible for anything
//////////////////////////////////////////////////////////////////////////////////////////////////

#include "DirectInput.h"

///////////////////////////////////////////////////////
// DirectInput Constructor
///////////////////////////////////////////////////////
DirectInput::DirectInput()
{
	//Nothing to construct
}

///////////////////////////////////////////////////////
// DirectInput Deconstructor
///////////////////////////////////////////////////////
DirectInput::~DirectInput()
{
	//Release the DInput, Keyboard, and Mouse devices
	DInput->Release(); DInput = NULL;
	Keyboard->Release(); Keyboard = NULL;
	Mouse->Release(); Mouse = NULL;
}

///////////////////////////////////////////////////////
// DirectInput Initialize
///////////////////////////////////////////////////////
void DirectInput::InitializeDirectInput(HINSTANCE appInstance, HWND hwnd, 
								   DWORD keyboardCoopFlags = DISCL_NONEXCLUSIVE|DISCL_FOREGROUND,
		                           DWORD mouseCoopFlags    = DISCL_NONEXCLUSIVE|DISCL_FOREGROUND)
{
	// Zero memory.
	ZeroMemory(KeyboardState, sizeof(KeyboardState));
	ZeroMemory(&MouseState, sizeof(MouseState));

	// Init DirectInput.
	DirectInput8Create( appInstance, DIRECTINPUT_VERSION, IID_IDirectInput8, (void**)&DInput, 0);

	// Init keyboard.
	DInput->CreateDevice(GUID_SysKeyboard, &Keyboard, 0);
	Keyboard->SetDataFormat(&c_dfDIKeyboard);
	Keyboard->SetCooperativeLevel(hwnd, keyboardCoopFlags);
	Keyboard->Acquire();

	// Init mouse.
	DInput->CreateDevice(GUID_SysMouse, &Mouse, 0);
	Mouse->SetDataFormat(&c_dfDIMouse2);
	Mouse->SetCooperativeLevel(hwnd, mouseCoopFlags);
	Mouse->Acquire();
}

///////////////////////////////////////////////////////
// Poll the DirectInput devices
///////////////////////////////////////////////////////
void DirectInput::Poll()
{
	// Poll keyboard
	HRESULT hr = Keyboard->GetDeviceState(sizeof(KeyboardState), 
		(void**)&KeyboardState); 

	if( FAILED(hr) )
	{
		// Keyboard lost, zero out keyboard data structure.
		ZeroMemory(KeyboardState, sizeof(KeyboardState));

		// Try to acquire for next time we poll.
		hr = Keyboard->Acquire();
	}

	// Poll mouse
	hr = Mouse->GetDeviceState(sizeof(DIMOUSESTATE2), 
		(void**)&MouseState); 

	if( FAILED(hr) )
	{
		// Mouse lost, zero out mouse data structure.
		ZeroMemory(&MouseState, sizeof(MouseState));

		// Try to acquire for next time we poll.
		hr = Mouse->Acquire(); 
	}
}

///////////////////////////////////////////////////////
// Keyboard Key Press
///////////////////////////////////////////////////////
bool DirectInput::KeyDown(char key)
{
	return (KeyboardState[key] & 0x80) != 0;
}

///////////////////////////////////////////////////////
// Mouse Key Press
///////////////////////////////////////////////////////
bool DirectInput::MouseButtonDown(int button)
{
	return (MouseState.rgbButtons[button] & 0x80) != 0;
}
import type { ReactNode } from "react";

import Logo from "../../../assets/images/logo.png";
import Background from "../../../assets/images/login-bg2.jpg";

import "./AuthLayout.css";

type Props = {
    children: ReactNode;
};

export default function AuthLayout({ children }: Props) {

    return (

        <div
            className="auth-container"
            style={{
                backgroundImage: `url(${Background})`
            }}
        >

            <div className="auth-overlay">

                {/* Left */}

                <div className="left-panel">

                    <img
                        src={Logo}
                        alt="Logo"
                        className="logo"
                    />

                    <h1>Yadgar Cafe</h1>

                    <p>
                        Smart Cafe Management System
                    </p>

                </div>

                {/* Right */}

                <div className="right-panel">

                    {children}

                </div>

            </div>

        </div>

    );

}
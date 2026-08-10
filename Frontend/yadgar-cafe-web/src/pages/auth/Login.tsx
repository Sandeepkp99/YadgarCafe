import Input from "../../components/common/Input";
import Button from "../../components/common/Button";
import AuthLayout from "../../components/layout/AuthLayout/AuthLayout";

export default function Login() {

    return (

        <AuthLayout>

            <div className="login-card">

                <h2>Welcome Back!</h2>

                <p>Please login to continue</p>

                <Input
                    label="Email"
                    type="email"
                    placeholder="Enter email"
                />

                <Input
                    label="Password"
                    type="password"
                    placeholder="Enter password"
                />

                <Button title="Login"/>

                <div className="bottom-links">

                    <a href="#">Forgot Password?</a>

    <a href="/register">
        Create Account
    </a>

                </div>

            </div>

        </AuthLayout>

    );

}
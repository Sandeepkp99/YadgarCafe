import Input from "../../components/common/Input";
import Button from "../../components/common/Button";
import AuthLayout from "../../components/layout/AuthLayout/AuthLayout";

export default function Register() {

    return (

        <AuthLayout>

            <div className="register-card">

                <h2>Create Account</h2>

                <p>Register to continue</p>

                <Input
                    label="Full Name"
                    placeholder="Enter full name"
                />

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

                <Input
                    label="Confirm Password"
                    type="password"
                    placeholder="Confirm Password"
                />

                <Button title="Register"/>

                <div className="bottom-links">

                    <span>Already have an account?</span>

                    <a href="/login">Login</a>

                </div>

            </div>

        </AuthLayout>

    );

}
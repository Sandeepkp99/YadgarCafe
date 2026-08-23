import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { toast } from 'react-toastify';
import Input from "../../components/common/Input";
import Button from "../../components/common/Button";
import AuthLayout from "../../components/layout/AuthLayout/AuthLayout";
import { useAuth } from '../../context/AuthContext';

const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function Login() {
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();
  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      await login(data.email, data.password);
      toast.success('Login successful!');
      navigate('/dashboard');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout>
      <div className="login-card">
        <h2>Welcome Back!</h2>
        <p>Please login to continue</p>

        <form onSubmit={handleSubmit(onSubmit)}>
          <Input
            label="Email"
            type="email"
            placeholder="Enter email"
            {...register('email')}
          />
          {errors.email && <span className="error">{errors.email.message}</span>}

          <Input
            label="Password"
            type="password"
            placeholder="Enter password"
            {...register('password')}
          />
          {errors.password && <span className="error">{errors.password.message}</span>}

          <Button title={isLoading ? 'Logging in...' : 'Login'} disabled={isLoading} />
        </form>

        <div className="bottom-links">
          <a href="#">Forgot Password?</a>
          <a href="/register">Create Account</a>
        </div>
      </div>
    </AuthLayout>
  );
}

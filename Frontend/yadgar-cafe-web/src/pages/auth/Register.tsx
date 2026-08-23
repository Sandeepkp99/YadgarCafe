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

const registerSchema = z.object({
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  email: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"],
});

type RegisterFormData = z.infer<typeof registerSchema>;

export default function Register() {
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { register: registerUser } = useAuth();
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    try {
      await registerUser(data.email, data.password, data.firstName, data.lastName);
      toast.success('Registration successful!');
      navigate('/dashboard');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Registration failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout>
      <div className="register-card">
        <h2>Create Account</h2>
        <p>Register to continue</p>

        <form onSubmit={handleSubmit(onSubmit)}>
          <Input
            label="First Name"
            placeholder="Enter first name"
            {...register('firstName')}
          />
          {errors.firstName && <span className="error">{errors.firstName.message}</span>}

          <Input
            label="Last Name"
            placeholder="Enter last name"
            {...register('lastName')}
          />
          {errors.lastName && <span className="error">{errors.lastName.message}</span>}

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

          <Input
            label="Confirm Password"
            type="password"
            placeholder="Confirm Password"
            {...register('confirmPassword')}
          />
          {errors.confirmPassword && <span className="error">{errors.confirmPassword.message}</span>}

          <Button title={isLoading ? 'Registering...' : 'Register'} disabled={isLoading} />
        </form>

        <div className="bottom-links">
          <span>Already have an account?</span>
          <a href="/login">Login</a>
        </div>
      </div>
    </AuthLayout>
  );
}

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import api from '@/services/api';
import { useAuth } from '@/hooks/useAuth';

const InitSchema = Yup.object().shape({
  tenantName: Yup.string().required('Required'),
  email: Yup.string().email('Invalid email').required('Required'),
  password: Yup.string()
    .min(8, 'Password must be at least 8 characters')
    .required('Required'),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref('password')], 'Passwords must match')
    .required('Required'),
});

export default function SystemInit() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [error, setError] = useState<string>();

  const { mutate, isLoading } = useMutation({
    mutationFn: async (values: {
      tenantName: string;
      email: string;
      password: string;
    }) => {
      // First create the tenant
      const tenantResponse = await api.post('/tenants/initialize', {
        name: values.tenantName,
      });

      // Then create the admin user
      const userResponse = await api.post('/auth/register', {
        email: values.email,
        password: values.password,
        tenantId: tenantResponse.data.id,
        role: 'admin',
      });

      return userResponse.data;
    },
    onSuccess: (data) => {
      login(data.user, data.token);
      navigate('/');
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'An error occurred during initialization');
    },
  });

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            System Initialization
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Set up your first tenant and admin account
          </p>
        </div>

        <Formik
          initialValues={{
            tenantName: '',
            email: '',
            password: '',
            confirmPassword: '',
          }}
          validationSchema={InitSchema}
          onSubmit={(values) => {
            setError(undefined);
            mutate({
              tenantName: values.tenantName,
              email: values.email,
              password: values.password,
            });
          }}
        >
          {({ errors, touched }) => (
            <Form className="mt-8 space-y-6">
              <div className="rounded-md shadow-sm -space-y-px">
                <div>
                  <label htmlFor="tenantName" className="sr-only">
                    Tenant Name
                  </label>
                  <Field
                    id="tenantName"
                    name="tenantName"
                    type="text"
                    className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm"
                    placeholder="Tenant Name"
                  />
                  {errors.tenantName && touched.tenantName && (
                    <div className="text-red-500 text-xs mt-1">{errors.tenantName}</div>
                  )}
                </div>

                <div>
                  <label htmlFor="email" className="sr-only">
                    Admin Email
                  </label>
                  <Field
                    id="email"
                    name="email"
                    type="email"
                    className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm"
                    placeholder="Admin Email"
                  />
                  {errors.email && touched.email && (
                    <div className="text-red-500 text-xs mt-1">{errors.email}</div>
                  )}
                </div>

                <div>
                  <label htmlFor="password" className="sr-only">
                    Password
                  </label>
                  <Field
                    id="password"
                    name="password"
                    type="password"
                    className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm"
                    placeholder="Password"
                  />
                  {errors.password && touched.password && (
                    <div className="text-red-500 text-xs mt-1">{errors.password}</div>
                  )}
                </div>

                <div>
                  <label htmlFor="confirmPassword" className="sr-only">
                    Confirm Password
                  </label>
                  <Field
                    id="confirmPassword"
                    name="confirmPassword"
                    type="password"
                    className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm"
                    placeholder="Confirm Password"
                  />
                  {errors.confirmPassword && touched.confirmPassword && (
                    <div className="text-red-500 text-xs mt-1">{errors.confirmPassword}</div>
                  )}
                </div>
              </div>

              {error && (
                <div className="rounded-md bg-red-50 p-4">
                  <div className="flex">
                    <div className="ml-3">
                      <h3 className="text-sm font-medium text-red-800">{error}</h3>
                    </div>
                  </div>
                </div>
              )}

              <div>
                <button
                  type="submit"
                  disabled={isLoading}
                  className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
                >
                  {isLoading ? 'Initializing...' : 'Initialize System'}
                </button>
              </div>
            </Form>
          )}
        </Formik>
      </div>
    </div>
  );
}
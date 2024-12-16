import { useQuery, useMutation } from '@tanstack/react-query';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import api from '@/services/api';
import { useAuth } from '@/hooks/useAuth';

const ProfileSchema = Yup.object().shape({
  firstName: Yup.string().required('Required'),
  lastName: Yup.string().required('Required'),
  email: Yup.string().email('Invalid email').required('Required'),
});

const PasswordSchema = Yup.object().shape({
  currentPassword: Yup.string().required('Required'),
  newPassword: Yup.string()
    .min(8, 'Password must be at least 8 characters')
    .required('Required'),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref('newPassword')], 'Passwords must match')
    .required('Required'),
});

export default function Settings() {
  const { user } = useAuth();

  const { data: profile, isLoading } = useQuery({
    queryKey: ['profile'],
    queryFn: () => api.get('/users/profile').then(res => res.data)
  });

  const { mutate: updateProfile } = useMutation({
    mutationFn: (values: { firstName: string; lastName: string; email: string }) =>
      api.put('/users/profile', values),
    onSuccess: () => {
      // Show success message
    },
  });

  const { mutate: changePassword } = useMutation({
    mutationFn: (values: { currentPassword: string; newPassword: string }) =>
      api.put('/users/change-password', values),
    onSuccess: () => {
      // Show success message
    },
  });

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-10 divide-y divide-gray-900/10">
      <div className="grid grid-cols-1 gap-x-8 gap-y-8 md:grid-cols-3">
        <div className="px-4 sm:px-0">
          <h2 className="text-base font-semibold leading-7 text-gray-900">Profile</h2>
          <p className="mt-1 text-sm leading-6 text-gray-600">Update your personal information.</p>
        </div>

        <div className="bg-white shadow-sm ring-1 ring-gray-900/5 sm:rounded-xl md:col-span-2">
          <Formik
            initialValues={{
              firstName: profile?.firstName || '',
              lastName: profile?.lastName || '',
              email: profile?.email || '',
            }}
            validationSchema={ProfileSchema}
            onSubmit={updateProfile}
          >
            {({ errors, touched }) => (
              <Form className="px-4 py-6 sm:p-8">
                <div className="grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                  <div className="sm:col-span-3">
                    <label htmlFor="firstName" className="block text-sm font-medium leading-6 text-gray-900">
                      First name
                    </label>
                    <Field
                      type="text"
                      name="firstName"
                      id="firstName"
                      className="mt-2 block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-primary-600 sm:text-sm sm:leading-6"
                    />
                    {errors.firstName && touched.firstName && (
                      <div className="mt-2 text-sm text-red-600">{errors.firstName}</div>
                    )}
                  </div>

                  <div className="sm:col-span-3">
                    <label htmlFor="lastName" className="block text-sm font-medium leading-6 text-gray-900">
                      Last name
                    </label>
                    <Field
                      type="text"
                      name="lastName"
                      id="lastName"
                      className="mt-2 block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-primary-600 sm:text-sm sm:leading-6"
                    />
                    {errors.lastName && touched.lastName && (
                      <div className="mt-2 text-sm text-red-600">{errors.lastName}</div>
                    )}
                  </div>

                  <div className="sm:col-span-4">
                    <label htmlFor="email" className="block text-sm font-medium leading-6 text-gray-900">
                      Email address
                    </label>
                    <Field
                      type="email"
                      name="email"
                      id="email"
                      className="mt-2 block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-primary-600 sm:text-sm sm:leading-6"
                    />
                    {errors.email && touched.email && (
                      <div className="mt-2 text-sm text-red-600">{errors.email}</div>
                    )}
                  </div>
                </div>

                <div className="mt-6 flex items-center justify-end gap-x-6">
                  <button
                    type="submit"
                    className="rounded-md bg-primary-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-primary-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-600"
                  >
                    Save
                  </button>
                </div>
              </Form>
            )}
          </Formik>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-x-8 gap-y-8 pt-10 md:grid-cols-3">
        <div className="px-4 sm:px-0">
          <h2 className="text-base font-semibold leading-7 text-gray-900">Change Password</h2>
          <p className="mt-1 text-sm leading-6 text-gray-600">
            Update your password to keep your account secure.
          </p>
        </div>

        <div className="bg-white shadow-sm ring-1 ring-gray-900/5 sm:rounded-xl md:col-span-2">
          <Formik
            initialValues={{
              currentPassword: '',
              newPassword: '',
              confirmPassword: '',
            }}
            validationSchema={PasswordSchema}
            onSubmit={changePassword}
          >
            {({ errors, touched }) => (
              <Form className="px-4 py-6 sm:p-8">
                <div className="grid grid-cols-1 gap-x-6 gap-y-8 sm:grid-cols-6">
                  <div className="sm:col-span-4">
                    <label htmlFor="currentPassword" className="block text-sm font-medium leading-6 text-gray-900">
                      Current Password
                    </label>
                    <Field
                      type="password"
                      name="currentPassword"
                      id="currentPassword"
                      className="mt-2 block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-primary-600 sm:text-sm sm:leading-6"
                    />
                    {errors.currentPassword && touched.currentPassword && (
                      <div className="mt-2 text-sm text-red-600">{errors.currentPassword}</div>
                    )}
                  </div>

                  <div className="sm:col-span-4">
                    <label htmlFor="newPassword" className="block text-sm font-medium leading-6 text-gray-900">
                      New Password
                    </label>
                    <Field
                      type="password"
                      name="newPassword"
                      id="newPassword"
                      className="mt-2 block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-primary-600 sm:text-sm sm:leading-6"
                    />
                    {errors.newPassword && touched.newPassword && (
                      <div className="mt-2 text-sm text-red-600">{errors.newPassword}</div>
                    )}
                  </div>

                  <div className="sm:col-span-4">
                    <label htmlFor="confirmPassword" className="block text-sm font-medium leading-6 text-gray-900">
                      Confirm New Password
                    </label>
                    <Field
                      type="password"
                      name="confirmPassword"
                      id="confirmPassword"
                      className="mt-2 block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-primary-600 sm:text-sm sm:leading-6"
                    />
                    {errors.confirmPassword && touched.confirmPassword && (
                      <div className="mt-2 text-sm text-red-600">{errors.confirmPassword}</div>
                    )}
                  </div>
                </div>

                <div className="mt-6 flex items-center justify-end gap-x-6">
                  <button
                    type="submit"
                    className="rounded-md bg-primary-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-primary-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-600"
                  >
                    Change Password
                  </button>
                </div>
              </Form>
            )}
          </Formik>
        </div>
      </div>
    </div>
  );
}
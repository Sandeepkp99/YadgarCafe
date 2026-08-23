import "./Input.css";
import { forwardRef, InputHTMLAttributes, ReactNode } from "react";

type Props = InputHTMLAttributes<HTMLInputElement> & {
    label: string;
    icon?: ReactNode;
}

export default forwardRef<HTMLInputElement, Props>(function Input({
    label,
    icon,
    type = "text",
    placeholder,
    ...props
}: Props, ref) {

    return (

        <div className="input-group">

            <label>

                {icon}

                <span>{label}</span>

            </label>

            <input
                ref={ref}
                type={type}
                placeholder={placeholder}
                {...props}
            />

        </div>

    );

})
